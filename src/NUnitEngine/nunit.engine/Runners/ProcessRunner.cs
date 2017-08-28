// ***********************************************************************
// Copyright (c) 2011-2014 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Engine.Agents;
using NUnit.Engine.Internal;

namespace NUnit.Engine.Runners
{
    /// <summary>
    /// Acquires and releases an agent from the given pool, adapting it
    /// to the <see cref="AbstractTestRunner"/> contract and handling errors.
    /// </summary>
    public sealed class ProcessRunner : AbstractTestRunner
    {
        private const int NORMAL_TIMEOUT = 30000;               // 30 seconds
        private const int DEBUG_TIMEOUT = NORMAL_TIMEOUT * 10;  // 5 minutes

        private static readonly Logger log = InternalTrace.GetLogger(typeof(ProcessRunner));

        private readonly IAgentPool _pool;
        private IAcquiredAgent _agent;
        private IAgentPackageContext _packageContext;
        private IAgentRunContext _runContext;

        public ProcessRunner(IServiceLocator services, TestPackage package) : base(services, package)
        {
            _pool = Services.GetService<IAgentPool>();
        }
        
        private IAcquiredAgent GetAgent()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ProcessRunner));

            if (_agent == null)
            {
                log.Info($"Acquiring agent for {TestPackage.Name}");

                // Increase the timeout to give time to attach a debugger
                var debug = TestPackage.GetSetting(EnginePackageSettings.DebugAgent, false) ||
                             TestPackage.GetSetting(EnginePackageSettings.PauseBeforeRun, false);

                _agent = _pool.AcquireAgent(AgentRequirements.GetRequirements(TestPackage), debug ? DEBUG_TIMEOUT : NORMAL_TIMEOUT);
            }
            return _agent;
        }

        private AgentPackageLoadResult GetPackageLoadResult()
        {
            var agent = GetAgent();
            log.Info($"Loading package {TestPackage.Name} into agent");
            return agent.LoadPackage(TestPackage);
        }

        private IAgentPackageContext GetPackageContext()
        {
            return _packageContext ?? (_packageContext = GetPackageLoadResult().Context);
        }

        protected override TestEngineResult LoadPackage()
        {
            try
            {
                UnloadPackage();

                var results = GetPackageLoadResult();
                _packageContext = results.Context;
                return results.LoadResult;
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", e.Message);
                return CreateFailedResult(TestPackage, e);
            }
        }

        public override void UnloadPackage()
        {
            try
            {
                if (_packageContext != null)
                {
                    StopRun(force: true);

                    log.Info("Unloading " + TestPackage.Name);
                    _packageContext.Dispose();
                    _packageContext = null;
                }
            }
            catch (Exception e) // TODO: Is this catch what we want?
            {
                log.Warning("Failed to unload the package. {0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Explore a <see cref="TestPackage"/> and return information about the tests found.
        /// </summary>
        /// <param name="filter">A <see cref="TestFilter"/> used to select tests.</param>
        public override TestEngineResult Explore(TestFilter filter)
        {
            try
            {
                return GetPackageContext().Explore(filter);
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", e.Message);
                return CreateFailedResult(TestPackage, e);
            }
        }

        /// <summary>
        /// Count the test cases that would be run under the specified filter.
        /// </summary>
        /// <param name="filter">A TestFilter</param>
        public override int CountTestCases(TestFilter filter)
        {
            try
            {
                return GetPackageContext().CountTestCases(filter);
            }
            catch (Exception e)
            {
                log.Error("Failed to count remote tests {0}", e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Run the tests in a loaded TestPackage
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        protected override TestEngineResult RunTests(ITestEventListener listener, TestFilter filter)
        {
            try
            {
                StopRun(force: true);

                log.Info("Running " + TestPackage.Name);

                _runContext = GetPackageContext().StartRun(filter);

                var eventHandler = listener == null ? null :
                    new EventHandler<TestEventArgs>((sender, e) => listener.OnTestEvent(e.Report));

                if (eventHandler != null) _runContext.TestEvent += eventHandler;
                try
                {
                    var result = _runContext.GetResult();
                    log.Info("Done running " + TestPackage.Name);
                    return result;
                }
                finally
                {
                    if (eventHandler != null) _runContext.TestEvent -= eventHandler;
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", e.Message);
                return CreateFailedResult(TestPackage, e);
            }
        }

        /// <summary>
        /// Start a run of the tests in the loaded TestPackage, returning immediately.
        /// The tests are run asynchronously and the listener interface is notified
        /// as it progresses.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        protected override AsyncTestEngineResult RunTestsAsync(ITestEventListener listener, TestFilter filter)
        {
            try
            {
                StopRun(force: true);

                log.Info("Running " + TestPackage.Name + " (async)");

                _runContext = GetPackageContext().StartRun(filter);

                var eventHandler = listener == null ? null :
                    new EventHandler<TestEventArgs>((sender, e) => listener.OnTestEvent(e.Report));

                if (eventHandler != null) _runContext.TestEvent += eventHandler;

                var asyncResult = new AsyncTestEngineResult();

                _runContext.OnCompleted(() =>
                {
                    _runContext.TestEvent -= eventHandler;
                    log.Info("Done running " + TestPackage.Name);
                    asyncResult.SetResult(_runContext.GetResult());
                });

                return asyncResult;
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", e.Message);
                var result = new AsyncTestEngineResult();
                result.SetResult(CreateFailedResult(TestPackage, e));
                return result;
            }
        }

        /// <summary>
        /// Cancel the ongoing test run. If no test is running, the call is ignored.
        /// </summary>
        /// <param name="force">If true, cancel any ongoing test threads, otherwise wait for them to complete.</param>
        public override void StopRun(bool force)
        {
            if (_runContext == null) return;

            try
            {
                _runContext.StopRun(force);
            }
            catch (Exception e)
            {
                log.Error("Failed to stop the remote run. {0}", e.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Disposal has to perform two actions, unloading the runner and
            // stopping the agent. Both must be tried even if one fails so
            // there can be up to two independent errors to be reported
            // through an NUnitEngineException. We do that by combining messages.
            if (!_disposed && disposing)
            {
                _disposed = true;

                string unloadError = null;

                try
                {
                    Unload();
                }
                catch(Exception ex)
                {
                    // Save and log the unload error
                    unloadError = ex.Message;
                    log.Error(unloadError);
                }

                if (_agent != null)
                {
                    try
                    {
                        log.Debug("Releasing remote agent back to the pool");
                        _agent.Dispose();
                        _agent = null;
                    }
                    catch (Exception e)
                    {
                        var stopError = $"Failed to release remote agent back to the pool. {e.Message}";
                        log.Error(stopError);
                        _agent = null;

                        // Stop error with no unload error, just rethrow
                        if (unloadError == null)
                            throw;

                        // Both kinds of errors, throw exception with combined message
                        throw new NUnitEngineException(unloadError + Environment.NewLine + stopError);
                    }
                }

                if (unloadError != null) // Add message line indicating we managed to stop agent anyway
                    throw (new NUnitEngineException(unloadError + "\nAgent was successfully released back to the pool after error."));
            }
        }

        private static TestEngineResult CreateFailedResult(TestPackage package, Exception e)
        {
            var suite = XmlHelper.CreateTopLevelElement("test-suite");
            suite.AddAttribute("type", "Assembly");
            suite.AddAttribute("id", package.ID);
            suite.AddAttribute("name", package.Name);
            suite.AddAttribute("fullname", package.FullName);
            suite.AddAttribute("runstate", "NotRunnable");
            suite.AddAttribute("testcasecount", "1");
            suite.AddAttribute("result", "Failed");
            suite.AddAttribute("label", "Error");
            suite.AddAttribute("start-time", DateTime.UtcNow.ToString("u"));
            suite.AddAttribute("end-time", DateTime.UtcNow.ToString("u"));
            suite.AddAttribute("duration", "0.001");
            suite.AddAttribute("total", "1");
            suite.AddAttribute("passed", "0");
            suite.AddAttribute("failed", "1");
            suite.AddAttribute("inconclusive", "0");
            suite.AddAttribute("skipped", "0");
            suite.AddAttribute("asserts", "0");

            var failure = suite.AddElement("failure");
            failure.AddElementWithCDataSection("message", e.Message);
            failure.AddElementWithCDataSection("stack-trace", e.StackTrace);

            return new TestEngineResult(suite);
        }
    }
}
