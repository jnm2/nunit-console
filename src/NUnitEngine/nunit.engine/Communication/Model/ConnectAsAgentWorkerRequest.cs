using System;
using System.Collections.Generic;
using System.IO;

namespace NUnit.Engine.Communication.Model
{
    internal struct ConnectAsAgentWorkerRequest
    {
        public ConnectAsAgentWorkerRequest(IDictionary<string, string> workerProperties)
        {
            WorkerProperties = workerProperties ?? throw new ArgumentNullException(nameof(workerProperties));
        }

        public IDictionary<string, string> WorkerProperties { get; }

        public static Result<ConnectAsAgentWorkerRequest> Read(ProtocolReader reader)
        {
            if (!reader.TryRead7BitEncodedInt32(out var propertyCount) || propertyCount < 0)
                return Result.Error("Invalid property count");

            var properties = new Dictionary<string, string>(propertyCount, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < propertyCount; i++)
            {
                var name = reader.ReadString();
                if (name.IsError(out var message)) return Result.Error(message);
                if (properties.ContainsKey(name.Value)) return Result.Error($"Multiple properties with the name '{name.Value}'");

                var value = reader.ReadString();
                if (value.IsError(out message)) return Result.Error(message);

                properties.Add(name.Value, value.Value);
            }

            return Result.Success(new ConnectAsAgentWorkerRequest(properties));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(WorkerProperties.Count);

            foreach (var property in WorkerProperties)
            {
                writer.Write(property.Key);
                writer.Write(property.Value);
            }
        }
    }
}
