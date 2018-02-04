using System;
using System.Reflection;

namespace NUnit.Engine.Internal.Metadata
{
    internal interface IAssemblyMetadataProvider : IDisposable
    {
        AssemblyName GetAssemblyName();
        ImageFileMachine PEMachine { get; }
        string MetadataVersionString { get; }
        CorFlags CorFlags { get; }
        AssemblyName[] GetAssemblyReferences();
        ICustomAttribute[] GetCustomAttributes();
        ITypeMetadata[] GetTypes();
    }

    internal interface ITypeMetadata
    {
        string FullName { get; }
    }
}
