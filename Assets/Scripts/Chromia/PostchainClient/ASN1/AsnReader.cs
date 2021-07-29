using System;
using System.Collections.Generic;


namespace Chromia.Postchain.Client.ASN1
{
    public class AsnReader
    {
        public int RemainingBytes {get {return _length - _readBytes;}}

        private Stack<byte> _bytes;
        private List<AsnReader> _sequences;
        private int _length = -1;
        private int _readBytes = 0;

        public AsnReader(byte[] bytes)
        {
            this._bytes = new Stack<byte>(bytes);
        }
        public AsnReader(ref Stack<byte> bytes, int length)
        {
            this._bytes = new Stack<byte>(bytes);
            this._length = length;
        }

        public byte PeekTag()
        {
            return _bytes.Peek();
        }

        public AsnReader ReadSequence()
        {
            GetByte(0x30);
            var length = ReadLength();

            return new AsnReader(ref this._bytes, length);
        }

        public byte[] ReadOctetString()
        {
            GetByte(0x04);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return buffer.ToArray();
        }

        public string ReadUTF8String()
        {
            GetByte(0x0c);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        }

        public int ReadInteger()
        {
            GetByte(0x02);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return BitConverter.ToInt32(buffer.ToArray(), 0);
        }

        private int ReadLength()
        {
            var first = GetByte();

            if (first < 128)
            {
                return first;
            }
            else
            {
                var byteAmount = first - 0x80;
                var bytes = new List<byte>();
                for (int i = 0; i < byteAmount; i++)
                {
                    bytes.Add(GetByte());
                }

                return BitConverter.ToInt32(bytes.ToArray(), 0);
            }
        }

        private byte GetByte(byte? expected = null)
        {
            if (_length > 0 && _readBytes >= _length)
            {
                throw new System.Exception("Tried to read more bytes than allowed");
            }
            else if (_bytes.Count == 0)
            {
                throw new System.Exception("No bytes left to read");
            }

            var got = _bytes.Peek();
            if (expected != got)
            {
                throw new System.Exception("Expected byte " + expected + ", got " + got);
            }

            _readBytes++;
            return  _bytes.Pop();
        }
    }
}