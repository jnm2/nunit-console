using System.IO;

namespace NUnit.Engine.Communication
{
    internal static class BinaryWriterExtensions
    {
        public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
        {
            Write7BitEncodedInt(writer, (uint)value);
        }

        public static void Write7BitEncodedInt(this BinaryWriter writer, uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        public static void Write7BitEncodedInt(this BinaryWriter writer, ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }
    }
}
