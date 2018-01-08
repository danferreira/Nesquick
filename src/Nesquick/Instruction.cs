using System;
using System.Reflection;

namespace Nesquick
{
     public class Instruction
    {
        public MethodInfo Method { get; set; }

        public byte OpCode { get; set; }

        public ushort Bytes { get; set; }

        public bool IncrementPC { get; set; }
    }
}