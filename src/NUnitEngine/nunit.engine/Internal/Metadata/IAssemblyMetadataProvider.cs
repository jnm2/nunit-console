using System.Collections.Generic;
using System.Reflection;

namespace NUnit.Engine.Internal.Metadata
{
    internal interface IAssemblyMetadataProvider
    {
        bool RequiresX86 { get; }
        string MetadataVersion { get; }

        AssemblyName AssemblyName { get; }
        AssemblyName[] AssemblyReferences { get; }

        bool HasAttribute(string fullAttributeTypeName);
        IEnumerable<AttributeMetadata> GetAttributes(string fullAttributeTypeName);

        IEnumerable<ITypeMetadataProvider> Types { get; }
    }
}
