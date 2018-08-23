using System;
using NUnit.Common;
using NUnit.Engine.Internal.Metadata;

namespace NUnit.Engine.Internal
{
    internal static class MetadataFacts
    {
        public static Version ParseMetadataVersion(string metadataVersion)
        {
            Guard.ArgumentNotNullOrEmpty(metadataVersion, nameof(metadataVersion));

            var version = metadataVersion;

            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                version = version.Substring(1);

            return new Version(version);
        }

        public static string GetTargetFramework(IAssemblyMetadataProvider assembly)
        {
            Guard.ArgumentNotNull(assembly, nameof(assembly));

            if (assembly.GetAttributes("System.Runtime.Versioning.TargetFrameworkAttribute").TryFirst(out var attr))
            {
                if (attr.PositionalArguments.FirstOrDefault() is string frameworkName)
                    return frameworkName;
            }

            foreach (var reference in assembly.AssemblyReferences)
            {
                if (reference.Name == "mscorlib" && BitConverter.ToUInt64(reference.GetPublicKeyToken(), 0) == 0xac22333d05b89d96)
                {
                    // We assume 3.5, since that's all we are supporting
                    // Could be extended to other versions if necessary
                    // Format for FrameworkName is invented - it is not
                    // known if any compilers supporting CF use the attribute
                    return ".NETCompactFramework,Version=3.5";
                }
            }

            return null;
        }

        public static bool RequiresAssemblyResolver(IAssemblyMetadataProvider assembly)
        {
            Guard.ArgumentNotNull(assembly, nameof(assembly));

            return assembly.HasAttribute("NUnit.Framework.TestAssemblyDirectoryResolveAttribute");
        }
    }
}
