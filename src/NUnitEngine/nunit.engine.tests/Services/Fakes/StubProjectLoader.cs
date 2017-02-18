using System.Collections.Generic;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.Tests.Fakes
{
    public sealed class StubProjectLoader : IProjectLoader
    {
        private readonly IDictionary<string, IProject> _projectsByPath;

        public StubProjectLoader(IDictionary<string, IProject> projectsByPath)
        {
            _projectsByPath = projectsByPath;
        }

        public bool CanLoadFrom(string path) => _projectsByPath.ContainsKey(path);

        public IProject LoadFrom(string path)
        {
            IProject project;
            return _projectsByPath.TryGetValue(path, out project) ? project : null;
        }
    }
}
