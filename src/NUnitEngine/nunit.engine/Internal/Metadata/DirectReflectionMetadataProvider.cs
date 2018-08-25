using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Common;

namespace NUnit.Engine.Internal.Metadata
{
    internal sealed partial class DirectReflectionMetadataProvider : IAssemblyMetadataProvider
    {
        private readonly string _path;
        private Assembly _assembly;

        public DirectReflectionMetadataProvider(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));

            _path = path;
        }

        private Assembly Assembly => _assembly ?? (_assembly = Assembly.ReflectionOnlyLoadFrom(_path));

        public bool RequiresX86
        {
            get
            {
                Assembly.ManifestModule.GetPEKind(out var peKind, out _);
                return (peKind & PortableExecutableKinds.Required32Bit) != 0;
            }
        }

        public string MetadataVersion => Assembly.ImageRuntimeVersion;

        public AssemblyName AssemblyName => Assembly.GetName();

        public AssemblyName[] AssemblyReferences => Assembly.GetReferencedAssemblies();

        public bool HasAttribute(string fullAttributeTypeName)
        {
            return GetAttributes(fullAttributeTypeName).Any();
        }

        public IEnumerable<AttributeMetadata> GetAttributes(string fullAttributeTypeName)
        {
            return DoWithReflectionOnlyAssemblyResolve(() =>
                GetAttributes(CustomAttributeData.GetCustomAttributes(Assembly), fullAttributeTypeName));
        }

        public IEnumerable<ITypeMetadataProvider> Types
        {
            get => Assembly.GetTypes().Select(type => (ITypeMetadataProvider)new TypeMetadataProvider(type));
        }

        private static IEnumerable<AttributeMetadata> GetAttributes(IEnumerable<CustomAttributeData> data, string fullAttributeTypeName)
        {
            foreach (var attribute in data)
            {
                if (attribute.Constructor.DeclaringType.FullName == fullAttributeTypeName)
                {
                    yield return new AttributeMetadata(
                        attribute.ConstructorArguments.ConvertAll(a => a.Value),
                        attribute.NamedArguments.ConvertAll(a => new NamedArgument(a.MemberInfo.Name, a.TypedValue.Value)));
                }
            }
        }

        private T DoWithReflectionOnlyAssemblyResolve<T>(Func<T> func)
        {
            Guard.ArgumentNotNull(func, nameof(func));

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
            try
            {
                return func.Invoke();
            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
            }

            Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
            {
                var fromSameDirectory = AssemblyMetadataProvider.TryResolveAssemblyPath(args.Name, Path.GetDirectoryName(_path));

                return fromSameDirectory != null
                    ? Assembly.ReflectionOnlyLoadFrom(fromSameDirectory)
                    : Assembly.ReflectionOnlyLoad(args.Name);
            }
        }
    }
}
