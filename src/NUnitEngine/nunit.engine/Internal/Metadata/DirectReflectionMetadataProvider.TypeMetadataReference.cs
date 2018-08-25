using System;
using System.Reflection;
using NUnit.Common;

namespace NUnit.Engine.Internal.Metadata
{
    partial class DirectReflectionMetadataProvider
    {
        private sealed class TypeMetadataReference : ITypeMetadataReference
        {
            private readonly Func<Type> _typeLoader;

            public TypeMetadataReference(Func<Type> typeLoader)
            {
                Guard.ArgumentNotNull(typeLoader, nameof(typeLoader));
                _typeLoader = typeLoader;
            }

            public ITypeMetadataProvider Resolve(Func<AssemblyName, string> assemblyPathResolver)
            {
                Guard.ArgumentNotNull(assemblyPathResolver, nameof(assemblyPathResolver));

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
                try
                {
                    return new TypeMetadataProvider(_typeLoader.Invoke());
                }
                finally
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= OnReflectionOnlyAssemblyResolve;
                }

                Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
                {
                    var fromSameDirectory = assemblyPathResolver.Invoke(new AssemblyName(args.Name));

                    return fromSameDirectory != null
                        ? Assembly.ReflectionOnlyLoadFrom(fromSameDirectory)
                        : Assembly.ReflectionOnlyLoad(args.Name);
                }
            }
        }
    }
}
