namespace NUnit.Engine.Agents
{
    public struct AgentRequirements
    {
        public static AgentRequirements None => default(AgentRequirements);

        public string RuntimeFrameworkId { get; set; }
        public bool? RunAsX86 { get; set; }
        public bool? DebugTests { get; set; }
        public bool? DebugAgent { get; set; }
        public string TraceLevel { get; set; }
        public bool? LoadUserProfile { get; set; }

        public static string DefaultRuntimeFrameworkId { get; } = RuntimeFramework.CurrentFramework.Id;
        public static bool DefaultRunAsX86 => false;
        public static bool DefaultDebugTests => false;
        public static bool DefaultDebugAgent => false;
        public static string DefaultTraceLevel => "Off";
        public static bool DefaultLoadUserProfile => false;

        public static AgentRequirements GetRequirements(TestPackage package)
        {
            var requirements = new AgentRequirements();

            var runtimeSetting = package.GetSetting(EnginePackageSettings.RuntimeFramework, "");
            if (runtimeSetting != "")
            {
                var targetRuntime = RuntimeFramework.Parse(runtimeSetting);
                if (targetRuntime.Runtime == RuntimeType.Any)
                    targetRuntime = new RuntimeFramework(RuntimeFramework.CurrentFramework.Runtime, targetRuntime.ClrVersion);
                requirements.RuntimeFrameworkId = targetRuntime.Id;
            }
            else
            {
                requirements.RuntimeFrameworkId = DefaultRuntimeFrameworkId;
            }

            requirements.RunAsX86 = package.GetSetting(EnginePackageSettings.RunAsX86, DefaultRunAsX86);
            requirements.DebugAgent = package.GetSetting(EnginePackageSettings.DebugAgent, DefaultDebugTests);
            requirements.DebugTests = package.GetSetting(EnginePackageSettings.DebugTests, DefaultDebugAgent);
            requirements.TraceLevel = package.GetSetting(EnginePackageSettings.InternalTraceLevel, DefaultTraceLevel);
            requirements.LoadUserProfile = package.GetSetting(EnginePackageSettings.LoadUserProfile, DefaultLoadUserProfile);

            return requirements;
        }
    }
}