using System.IO;

namespace NUnit.Engine.Communication.Model
{
    internal struct ProtocolHeader
    {
        private const string Tag = "NUnit";

        public static Result<ProtocolHeader> Read(ProtocolReader reader)
        {
            foreach (var headerByte in Tag)
            {
                if (!reader.TryReadByte(out var value) || value != headerByte)
                    return Result.Error("Unrecognized header");
            }

            return Result.Success(default(ProtocolHeader));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Tag.ToCharArray());
        }
    }
}
