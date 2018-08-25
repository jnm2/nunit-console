using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Common;

namespace NUnit.Engine.Internal.Metadata
{
    partial class DirectReflectionMetadataProvider
    {
        private sealed class TypeMetadataProvider : ITypeMetadataProvider
        {
            private readonly Type _type;

            public TypeMetadataProvider(Type type)
            {
                Guard.ArgumentNotNull(type, nameof(type));
                _type = type;
            }

            public string FullName => _type.FullName;

            public IEnumerable<AttributeMetadata> GetAttributes(string fullAttributeTypeName)
            {
                return DirectReflectionMetadataProvider.GetAttributes(
                    CustomAttributeData.GetCustomAttributes(_type),
                    fullAttributeTypeName);
            }

            public IEnumerable<ITypeMetadataReference> Interfaces
            {
                get => _type.GetInterfaces().Select(t => (ITypeMetadataReference)new TypeMetadataReference(() => t));
            }

            public ITypeMetadataReference BaseType
            {
                get => _type.BaseType == null ? null : new TypeMetadataReference(() => _type.BaseType);
            }
        }
    }
}
