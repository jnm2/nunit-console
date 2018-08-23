using System.Reflection;

namespace NUnit.Engine.Internal.Metadata
{
    internal interface ITypeMetadataReference
    {
        string FullName { get; }
        ITypeMetadataProvider Resolve(Func<AssemblyName, string> assemblyPathResolver);
    }
}
