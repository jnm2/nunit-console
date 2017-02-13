using System;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.Tests
{
    public static class ExtensionServiceTestExtensions
    {
        // extensionInstances is object instead of T so that you are forced to explicitly specify <T> at the call site.
        public static ExtensionNode InstallExtensionInstance<T>(this ExtensionService extensionService, object extensionInstance)
        {
            if (extensionService.Status == ServiceStatus.Stopped)
                extensionService.StartService();

            var extensionPoint = extensionService.GetExtensionPoint(typeof(T));
            if (extensionPoint == null)
                throw new InvalidOperationException($"No extension point {typeof(T).FullName} is loaded.");

            var node = new InstanceExtensionNode(extensionInstance) { Path = extensionPoint.Path };

            extensionPoint.Install(node);

            return node;
        }

        private sealed class InstanceExtensionNode : ExtensionNode
        {
            private readonly object _instance;

            public InstanceExtensionNode(object instance) : base(instance.GetType().Assembly.Location, instance.GetType().FullName)
            {
                _instance = instance;
            }

            public override object CreateExtensionObject(params object[] args)
            {
                return _instance;
            }
        }
    }
}
