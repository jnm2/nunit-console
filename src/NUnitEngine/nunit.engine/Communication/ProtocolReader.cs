using System;
using System.IO;

namespace NUnit.Engine.Communication
{
    internal struct ProtocolReader
    {
        private readonly BinaryReader _reader;

        public ProtocolReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public bool TryReadByte(out byte value)
        {
            var result = _reader.Read();
            if (result != -1)
            {
                value = (byte)result;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TryRead7BitEncodedInt32(out int value)
        {
            if (TryRead7BitEncodedUInt32(out var unsigned))
            {
                value = (int)unsigned;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TryRead7BitEncodedUInt32(out uint value)
        {
            var result = 0U;
            var shift = 0;

            while (TryReadByte(out var nextByte))
            {
                result |= (uint)(nextByte & 0x7F) << shift;

                if ((nextByte & 0x80) == 0)
                {
                    value = result;
                    return true;
                }

                shift += 7;
                if (shift > sizeof(uint) - 7) break;
            }

            value = 0;
            return false;
        }

        public Result<string> ReadString()
        {
            try
            {
                return Result.Success(_reader.ReadString());
            }
            catch (Exception ex) when (ex is FormatException || ex is IOException)
            {
                return Result.Error("Error parsing length-prefixed string: " + ex.Message);
            }
        }
    }
}
