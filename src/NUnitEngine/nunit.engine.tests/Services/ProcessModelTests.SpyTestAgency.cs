using System;
using System.Collections.Generic;
using NUnit.Engine.Services.Tests.Fakes;

namespace NUnit.Engine.Services.Tests
{
    partial class ProcessModelTests
    {
        private sealed class SpyTestAgency : FakeService, ITestAgency
        {
            public List<CreatedProcess> CreatedProcesses { get; } = new List<CreatedProcess>();

            void ITestAgency.Register(ITestAgent agent)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// <see cref="TestAgency.GetAgent"/> creates a process during this call
            /// </summary>
            ITestAgent ITestAgency.GetAgent(TestPackage package, int waitTime)
            {
                var createdProcess = new CreatedProcess(this);
                CreatedProcesses.Add(createdProcess);
                return createdProcess;
            }

            public sealed class CreatedProcess : ITestAgent
            {
                private readonly ITestAgency _agency;

                public CreatedProcess(ITestAgency agency)
                {
                    _agency = agency;
                }

                public List<TestPackage> ExecutedPackages { get; } = new List<TestPackage>();

                ITestAgency ITestAgent.Agency => _agency;

                Guid ITestAgent.Id { get; } = Guid.NewGuid();

                bool ITestAgent.Start() => true;

                void ITestAgent.Stop() { }

                ITestEngineRunner ITestAgent.CreateRunner(TestPackage package)
                {
                    ExecutedPackages.Add(package);
                    return new DummyRunner();
                }

                private sealed class DummyRunner : ITestEngineRunner
                {
                    public void Dispose() { }

                    public TestEngineResult Load() => new TestEngineResult();

                    public void Unload() { }

                    public TestEngineResult Reload() => null;

                    public int CountTestCases(TestFilter filter) => 0;

                    public TestEngineResult Run(ITestEventListener listener, TestFilter filter) => new TestEngineResult();

                    public AsyncTestEngineResult RunAsync(ITestEventListener listener, TestFilter filter)
                    {
                        var r = new AsyncTestEngineResult();
                        r.SetResult(new TestEngineResult());
                        return r;
                    }

                    public void StopRun(bool force) { }

                    public TestEngineResult Explore(TestFilter filter) => new TestEngineResult();
                }
            }
        }
    }
}