using System;

namespace Nesquick.Tests
{
    public class NestestConsole : NESConsole
    {
        public NestestConsole(Cartridge rom) : base(rom)
        {
        }

        protected override void Setup()
        {
            CPU.Reset();
            CPU._PC = 0xC000;
            CPU._P = 0x24;
            CPU.Memory.Write(0x8000, ROM.PRGRomData);
            CPU.Memory.Write(0xC000, ROM.PRGRomData);
        }

        protected override void WriteLog() {
            //Empty
        }
    }
}