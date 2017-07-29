using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Leayal.VData
{
    public static class Xor
    {
        public const System.Int32 SecretByte = 0x55;
    }
    public class XorStream : System.IO.FileStream
    {
        public XorStream(string Path) : base(Path, FileMode.OpenOrCreate)
        {
        }

        public XorStream(string Path, FileMode fileMode, FileAccess fileAccess) : base(Path, fileMode, fileAccess)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int theRead = base.Read(buffer, offset, count);
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= Xor.SecretByte;
            return theRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= Xor.SecretByte;
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            value ^= Xor.SecretByte;
            base.WriteByte(value);
        }

        public override int ReadByte()
        {
            return (base.ReadByte() ^ Xor.SecretByte);
        }        
    }
}
