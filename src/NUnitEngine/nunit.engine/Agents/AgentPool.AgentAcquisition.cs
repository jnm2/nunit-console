using System;

namespace NUnit.Engine.Agents
{
    partial class AgentPool
    {
        private sealed class AgentAcquisition : IAcquiredAgent
        {
            private readonly AgentPool _pool;
            private readonly Agent _agent;
            private bool isDisposed;

            public AgentAcquisition(AgentPool pool, Agent agent)
            {
                _pool = pool;
                _agent = agent;
            }

            public AgentPackageLoadResult LoadPackage(TestPackage testPackage)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                if (isDisposed) return;
                isDisposed = true;

                _pool.ReleaseAgent(_agent);

                GC.SuppressFinalize(this);
            }

            ~AgentAcquisition()
            {
                _pool.ReleaseAgent(_agent);
            }
        }
    }
}