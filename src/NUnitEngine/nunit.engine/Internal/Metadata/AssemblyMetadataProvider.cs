using System;
using System.IO;

namespace NUnit.Engine.Internal.Metadata
{
    internal static class AssemblyMetadataProvider
    {
        public static IAssemblyMetadataProvider Create(string assemblyPath)
        {
            throw new NotImplementedException();
        }

        public static string TryResolveAssemblyPath(string assemblyName, string directory)
        {
            var path = Path.Combine(directory, assemblyName + ".dll");
            if (File.Exists(path)) return path;

            path = Path.Combine(directory, assemblyName + ".exe");
            if (File.Exists(path)) return path;

            return null;
        }
    }
}
