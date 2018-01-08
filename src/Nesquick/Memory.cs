using System;

namespace Nesquick
{
    public class Memory
    {
        byte[] RAM;

        public Memory() => RAM = new byte[65536];

        public byte Read8(ushort address) => RAM[address];

        public ushort Read16(ushort address, bool pageWrap = false)
        {
            var b1 = RAM[address];
            var b2 = pageWrap && (address & 0xFF) == 0xFF ? RAM[address & 0xFF00] : RAM[address + 1];

            return (ushort)((b2 << 8) | b1);

            //throw new NotImplementedException();
        }

        public byte Write(ushort address, byte value) => RAM[address] = value;

        public void Write(ushort address, byte[] value) => Array.Copy(value, 0, RAM, address, value.Length);
    }
}