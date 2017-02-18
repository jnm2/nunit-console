using System.Collections.Generic;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.Tests.Fakes
{
    public sealed class StubProject : IProject
    {
        private readonly TestPackage _testPackage;

        public StubProject(string projectPath, string configName, TestPackage testPackage)
        {
            ProjectPath = projectPath;
            ActiveConfigName = configName;
            _testPackage = testPackage;
        }

        public string ProjectPath { get; }

        public string ActiveConfigName { get; }

        public IList<string> ConfigNames => new[] { ActiveConfigName };

        public TestPackage GetTestPackage() => _testPackage;

        public TestPackage GetTestPackage(string configName) => _testPackage;
    }
}