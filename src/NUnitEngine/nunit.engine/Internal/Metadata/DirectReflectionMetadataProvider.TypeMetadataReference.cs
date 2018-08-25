using System;
using System.Reflection;
using NUnit.Common;

namespace NUnit.Engine.Internal.Metadata
{
    partial class DirectReflectionMetadataProvider
    {
        private sealed class TypeMetadataReference : ITypeMetadataReference
        {
            private readonly Type _referencedType;

            public TypeMetadataReference(Type referencedReferencedType)
            {
                Guard.ArgumentNotNull(referencedReferencedType, nameof(referencedReferencedType));
                _referencedType = referencedReferencedType;
            }

            public string FullName => _referencedType.FullName;

            public ITypeMetadataProvider Resolve(Func<AssemblyName, string> assemblyPathResolver)
            {
                return new TypeMetadataProvider(_referencedType);
            }
        }
    }
}
