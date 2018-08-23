using System.Collections.Generic;

namespace NUnit.Engine.Internal.Metadata
{
    internal interface ITypeMetadataProvider
    {
        string FullName { get; }
        IEnumerable<AttributeMetadata> GetAttributes(string fullAttributeTypeName);
        IEnumerable<ITypeMetadataReference> Interfaces { get; }
        ITypeMetadataReference BaseType { get; }
    }
}
