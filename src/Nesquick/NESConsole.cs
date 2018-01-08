using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nesquick
{
    public class NESConsole
    {
        private List<Instruction> instructions;
        public Cartridge ROM { get; private set; }
        public CPU CPU { get; private set; }
        public StringBuilder Log { get; private set; }

        public NESConsole(Cartridge rom)
        {
            ROM = rom;

            CPU = new CPU();
            instructions = new List<Instruction>();
            Log = new StringBuilder();

            LoadInstructions();
        }
        private void LoadInstructions()
        {
            var methods = CPU.GetType().GetMethods();

            foreach (var method in methods)
            {
                var attribute = Attribute.GetCustomAttribute(method, typeof(OpCodeAttribute), false) as OpCodeAttribute;
                if (attribute == null)
                    continue;

                instructions.Add(new Instruction { Method = method, Bytes = attribute.Bytes, OpCode = attribute.Opcode, IncrementPC = attribute.IncrementPC });
            }
        }
        protected virtual void Setup()
        {
            CPU.Reset();
            CPU._PC = 0xC000;
            CPU._P = 0x24;
            CPU.Memory.Write(0x8000, ROM.PRGRomData);
            CPU.Memory.Write(0xC000, ROM.PRGRomData);
        }

        public void Run()
        {
            Setup();
            var i = 5000;
            
            while (i-- > 0)
            {
                var b = CPU.Memory.Read8(CPU._PC);

                var m = instructions.FirstOrDefault(c => (byte)c.OpCode == b);

                if (m == null)
                {
                    WriteLog();

                    throw new Exception("OpCode not implemented: " + b.ToString("X2"));
                }

                var currentPC = string.Format("{0:X4}", CPU._PC);
                var currentOpCode = string.Format("{0:X2}", m.OpCode);

                if (m.Bytes >= 2)
                    currentOpCode += string.Format(" {0:X2}", CPU.Memory.Read8((ushort)(CPU._PC + 1)));

                if (m.Bytes == 3)
                    currentOpCode += string.Format(" {0:X2}", CPU.Memory.Read8((ushort)(CPU._PC + 2)));

                var registers = string.Format("A:{0:X2} X:{1:X2} Y:{2:X2} P:{3:X2} SP:{4:X2}", CPU._A, CPU._X, CPU._Y, CPU._P, CPU._SP);

                m.Method.Invoke(CPU, null);

                //var lastInstruction = string.Format("{0} {1}", m.Method.Name.Substring(0, 3), CPU.LastAddress);

                var logLine = string.Format("{0, -6}{1, -9}{2, -4}{3}", currentPC, currentOpCode, m.Method.Name.Substring(0, 3), registers);
                Log.AppendLine(logLine);

                if (m.IncrementPC)
                    CPU._PC += m.Bytes;
            }

            WriteLog();
        }

        protected virtual void WriteLog() => System.IO.File.WriteAllLines(@"Roms/Log.log", Log.ToString().Split(Environment.NewLine));
    }
}