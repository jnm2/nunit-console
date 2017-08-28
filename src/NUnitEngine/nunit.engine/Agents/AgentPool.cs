using System;
using System.Collections.Generic;

namespace NUnit.Engine.Agents
{
    internal sealed partial class AgentPool : IAgentPool, IService
    {
        private readonly List<Agent> _availableAgents = new List<Agent>();
        private bool _isDisposed;

        public void Dispose()
        {
            Status = ServiceStatus.Stopped;

            lock (_availableAgents)
            {
                if (_isDisposed) return;
                _isDisposed = true;

                foreach (var agent in _availableAgents)
                    agent.Dispose();
                _availableAgents.Clear();
            }
        }

        public IAcquiredAgent AcquireAgent(AgentRequirements requirements, int timeoutMilliseconds)
        {
            lock (_availableAgents)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(AgentPool));

                for (var i = 0; i < _availableAgents.Count; i++)
                {
                    var agent = _availableAgents[i];
                    if (!agent.Matches(requirements)) continue;
                    _availableAgents.RemoveAt(i);
                    return new AgentAcquisition(this, agent);
                }
            }

            // Don't hold the lock while starting an agent
            var newAgent = Agent.Start(
                requirements.RuntimeFrameworkId ?? AgentRequirements.DefaultRuntimeFrameworkId,
                requirements.RunAsX86 ?? AgentRequirements.DefaultRunAsX86,
                requirements.DebugTests ?? AgentRequirements.DefaultDebugTests,
                requirements.DebugAgent ?? AgentRequirements.DefaultDebugAgent,
                requirements.TraceLevel ?? AgentRequirements.DefaultTraceLevel,
                requirements.LoadUserProfile ?? AgentRequirements.DefaultLoadUserProfile);

            lock (_availableAgents)
            {
                if (_isDisposed)
                {
                    newAgent.Dispose();
                    throw new ObjectDisposedException(nameof(AgentPool));
                }
            }

            return new AgentAcquisition(this, newAgent);
        }

        private void ReleaseAgent(Agent agent)
        {
            lock (_availableAgents)
            {
                if (_isDisposed)
                    agent.Dispose();
                else
                    _availableAgents.Add(agent);
            }
        }

        public IServiceLocator ServiceContext { get; set; }

        public ServiceStatus Status { get; private set; } = ServiceStatus.Started;

        public void StartService()
        {
            if (_isDisposed) Status = ServiceStatus.Error;
        }

        public void StopService()
        {
            Dispose();
        }
    }
}