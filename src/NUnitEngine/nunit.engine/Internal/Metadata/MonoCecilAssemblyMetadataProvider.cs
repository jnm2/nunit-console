using System;
using System.Globalization;
using System.Reflection;
using Mono.Cecil;

namespace NUnit.Engine.Internal.Metadata
{
    internal sealed class MonoCecilAssemblyMetadataProvider : IAssemblyMetadataProvider
    {
        private readonly AssemblyDefinition _assemblyDef;

        public MonoCecilAssemblyMetadataProvider(string assemblyPath)
        {
            _assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath);
        }

        public CorFlags CorFlags => (CorFlags)_assemblyDef.MainModule.Attributes;

        public AssemblyName GetAssemblyName() => FromDefinition(_assemblyDef.Name);

        public ImageFileMachine PEMachine
        {
            get
            {
                switch (_assemblyDef.MainModule.Architecture)
                {
                    case TargetArchitecture.I386:
                        return ImageFileMachine.I386;
                    case TargetArchitecture.IA64:
                        return ImageFileMachine.IA64;
                    case TargetArchitecture.AMD64:
                        return ImageFileMachine.AMD64;
                    case TargetArchitecture.ARMv7:
                        return (ImageFileMachine)0x01c4;
                    default:
                        throw new NotSupportedException(_assemblyDef.MainModule.Architecture.ToString());
                }
            }
        }

        public string MetadataVersionString => _assemblyDef.MainModule.RuntimeVersion;

        public AssemblyName[] GetAssemblyReferences()
        {
            var referenceDefs = _assemblyDef.MainModule.AssemblyReferences;
            var r = new AssemblyName[referenceDefs.Count];

            for (var i = 0; i < r.Length; i++)
                r[i] = FromDefinition(referenceDefs[i]);

            return r;
        }

        private static AssemblyName FromDefinition(AssemblyNameReference definition)
        {
            var r = new AssemblyName
            {
                Name = definition.Name,
                Version = definition.Version
            };

            if (definition.Culture != null) r.CultureInfo = CultureInfo.GetCultureInfo(definition.Culture);

            if (definition.HasPublicKey)
                r.SetPublicKey(definition.PublicKey);
            else if (definition.PublicKeyToken != null)
                r.SetPublicKeyToken(definition.PublicKeyToken);

            return r;
        }

        public ICustomAttribute[] GetCustomAttributes()
        {
            var attributeDefs = _assemblyDef.CustomAttributes;
            var r = new ICustomAttribute[attributeDefs.Count];

            for (var i = 0; i < r.Length; i++)
                r[i] = new CustomAttribute(attributeDefs[i]);

            return r;
        }

        private sealed class CustomAttribute : ICustomAttribute
        {
            private readonly Mono.Cecil.CustomAttribute _attributeDef;

            public CustomAttribute(Mono.Cecil.CustomAttribute attributeDef)
            {
                _attributeDef = attributeDef;
            }

            public string TypeFullName => _attributeDef.AttributeType.FullName;

            public object[] GetConstructorArguments()
            {
                var argumentDefs = _attributeDef.ConstructorArguments;
                var r = new object[argumentDefs.Count];

                for (var i = 0; i < r.Length; i++)
                    r[i] = argumentDefs[i].Value;

                return r;
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
