using System;

namespace NUnit.Engine.Agents
{
    public interface IAgentPool : IDisposable
    {
        IAcquiredAgent AcquireAgent(AgentRequirements requirements, int timeoutMilliseconds);
    }

    public interface IAcquiredAgent : IDisposable
    {
        AgentPackageLoadResult LoadPackage(TestPackage testPackage);
    }

    public struct AgentPackageLoadResult
    {
        public TestEngineResult LoadResult { get; }
        public IAgentPackageContext Context { get; }

        public AgentPackageLoadResult(TestEngineResult loadResult, IAgentPackageContext context)
        {
            LoadResult = loadResult;
            Context = context;
        }
    }

    public interface IAgentPackageContext : IDisposable
    {
        TestEngineResult Explore(TestFilter filter);
        int CountTestCases(TestFilter filter);
        IAgentRunContext StartRun(TestFilter filter);
    }

    public interface IAgentRunContext
    {
        bool IsCompleted { get; }
        void OnCompleted(Action continuation);
        TestEngineResult GetResult();

        event EventHandler<TestEventArgs> TestEvent;

        void StopRun(bool force);
    }

    public sealed class TestEventArgs : EventArgs
    {
        public TestEventArgs(string report)
        {
            Report = report;
        }

        public string Report { get; }
    }
}
