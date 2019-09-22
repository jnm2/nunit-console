using System.IO;

namespace NUnit.Engine.Communication.Model
{
    internal struct ProtocolVersion
    {
        public ProtocolVersion(byte major, byte minor, byte patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public byte Major { get; }
        public byte Minor { get; }
        public byte Patch { get; }

        public static Result<ProtocolVersion> Read(ProtocolReader reader)
        {
            if (reader.TryReadByte(out var major)
                && reader.TryReadByte(out var minor)
                && reader.TryReadByte(out var patch))
            {
                return Result.Success(new ProtocolVersion(major, minor, patch));
            }

            return Result.Error("Stream ended");
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Major);
            writer.Write(Minor);
            writer.Write(Patch);
        }
    }
}
