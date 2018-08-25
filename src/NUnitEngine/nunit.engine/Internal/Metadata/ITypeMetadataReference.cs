using System;
using System.Reflection;

namespace NUnit.Engine.Internal.Metadata
{
    internal interface ITypeMetadataReference
    {
        ITypeMetadataProvider Resolve(Func<AssemblyName, string> assemblyPathResolver);
    }
}
