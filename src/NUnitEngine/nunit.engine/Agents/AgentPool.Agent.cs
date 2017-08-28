using System;
using System.Diagnostics;

namespace NUnit.Engine.Agents
{
    partial class AgentPool
    {
        private sealed class Agent : IDisposable
        {
            private readonly string _runtimeFrameworkId;
            private readonly bool _runAsX86;
            private readonly bool _debugTests;
            private readonly bool _debugAgent;
            private readonly string _traceLevel;
            private readonly bool _loadUserProfile;
            private readonly Process _process;

            private Agent(string runtimeFrameworkId, bool runAsX86, bool debugTests, bool debugAgent, string traceLevel, bool loadUserProfile, Process process)
            {
                _runtimeFrameworkId = runtimeFrameworkId;
                _runAsX86 = runAsX86;
                _debugTests = debugTests;
                _debugAgent = debugAgent;
                _traceLevel = traceLevel;
                _loadUserProfile = loadUserProfile;
                _process = process;
            }

            public bool Matches(AgentRequirements requirements)
            {
                return (requirements.RuntimeFrameworkId == null || requirements.RuntimeFrameworkId == _runtimeFrameworkId)
                       && (requirements.RunAsX86 == null || requirements.RunAsX86.Value == _runAsX86)
                       && (requirements.DebugTests == null || requirements.DebugTests.Value == _debugTests)
                       && (requirements.DebugAgent == null || requirements.DebugAgent.Value == _debugAgent)
                       && (requirements.TraceLevel == null || requirements.TraceLevel == _traceLevel)
                       && (requirements.LoadUserProfile == null || requirements.LoadUserProfile.Value == _loadUserProfile);
            }

            public static Agent Start(string runtimeFrameworkId, bool runAsX86, bool debugTests, bool debugAgent, string traceLevel, bool loadUserProfile)
            {
                if (runtimeFrameworkId == null) throw new ArgumentNullException(nameof(runtimeFrameworkId));
                if (traceLevel == null) throw new ArgumentNullException(nameof(traceLevel));

                var process = new Process();

                throw new NotImplementedException();

                // return new Agent(runtimeFrameworkId, runAsX86, debugTests, debugAgent, traceLevel, loadUserProfile, process);
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}