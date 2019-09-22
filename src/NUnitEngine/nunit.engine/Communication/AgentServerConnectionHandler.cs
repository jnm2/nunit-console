using NUnit.Engine.Communication.Model;
using System;
using System.IO;

namespace NUnit.Engine.Communication
{
    internal sealed class AgentServerConnectionHandler
    {
        private readonly IAgentServer _server;

        public AgentServerConnectionHandler(IAgentServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public void HandleConnection(Stream stream)
        {
            // Disposing the writer guarantees that the writer is flushed before the connection is closed. TcpServer
            // can't know about the internal buffers in BinaryWriter.
            using (var writer = new BinaryWriter(stream))
            {
                HandleConnection(new ProtocolReader(stream), writer);
            }
        }

        private void HandleConnection(ProtocolReader reader, BinaryWriter writer)
        {
            if (ProtocolHeader.Read(reader).IsError())
            {
                // The incoming connection is not using this protocol. We should probably shut this connection down
                // without replying since we can't know how it will be interpreted. It could have an arbitrarily
                // significant effect.
                return;
            }

            var versionResult = CheckVersion(reader);
            if (versionResult.Code != RequestStatusCode.Success)
            {
                // The incoming connection is using a version of this protocol that is arbitrarily different. Even if we
                // can assume that what we just parsed is even a version, we can't assume that anything we send back
                // will be understood. However, we can assume that we are talking to an NUnit tool and that the risk of
                // something bad happening due to misinterpretation is super low.
                versionResult.Write(writer);
                return;
            }

            while (reader.TryReadByte(out var type))
            {
                switch ((AgentServerRequestType)type)
                {
                    case AgentServerRequestType.ConnectAsAgentWorker:
                    {
                        var request = ConnectAsAgentWorkerRequest.Read(reader);
                        if (request.IsError(out var message))
                        {
                            RequestStatus.Error(RequestStatusCode.ProtocolError, message).Write(writer);
                            return;
                        }

                        var status = _server.ConnectAsAgentWorker(request.Value);
                        if (status.Code != RequestStatusCode.Success)
                            status.Write(writer);
                        break;
                    }
                    default:
                    {
                        RequestStatus.Error(
                            RequestStatusCode.ProtocolError,
                            "Unknown command type")
                            .Write(writer);
                        return;
                    }
                }
            }
        }

        private static RequestStatus CheckVersion(ProtocolReader reader)
        {
            var version = ProtocolVersion.Read(reader);
            if (version.IsError(out var message))
                return RequestStatus.Error(RequestStatusCode.ProtocolError, message);

            // Other constraints could be added here as they arise. As a starting point, we are tying protocol version
            // to engine version. This might need more thought. We might want to be able make a breaking change to the
            // protocol without making a breaking change to the engine assembly. In that case, old existing versions
            // will need a way to tell that they are incompatible.
            const int maxMajorVersion = 3;
            if (version.Value.Major > maxMajorVersion)
            {
                return RequestStatus.Error(
                    RequestStatusCode.UnsupportedProtocolVersion,
                    $"Versions later than {maxMajorVersion} are not supported.");
            };

            return RequestStatus.Success;
        }
    }
}
