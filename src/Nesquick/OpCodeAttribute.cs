using System;

namespace Nesquick
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class OpCodeAttribute : Attribute
    {
        public byte Opcode { get; set; }

        public ushort Bytes { get; set; } = 1;

        public ushort Cycles { get; set; } = 2;

        public bool IncrementPC { get; set; } = true;
    }
}