using System;
using System.IO;

namespace Nesquick
{
    public class Cartridge
    {
        public byte[] PRGRomData { get; private set; }
        public byte[] CHRRomData { get; private set; }

        public Cartridge(string path) => LoadRom(path);

        private void LoadRom(string path)
        {
            var rom = File.ReadAllBytes(path);

            var signature = BitConverter.ToUInt32(rom, 0);

            if (signature != 0x1A53454E)
                throw new Exception("Not a valid iNES ROM");

            //Size of PRG ROM in 16 KB units
            var PRGROMSize = rom[4] * 0x4000;

            //Size of CHR ROM in 8 KB units (Value 0 means the board uses CHR RAM)
            if (rom[5] == 0)
                throw new Exception("CHR RAM instead of CHR ROM");

            var CHRROMSize = rom[5] * 0x2000;

            /*
                76543210
                ||||||||
                |||||||+- Mirroring: 0: horizontal (vertical arrangement) (CIRAM A10 = PPU A11)
                |||||||              1: vertical (horizontal arrangement) (CIRAM A10 = PPU A10)
                ||||||+-- 1: Cartridge contains battery-backed PRG RAM ($6000-7FFF) or other persistent memory
                |||||+--- 1: 512-byte trainer at $7000-$71FF (stored before PRG data)
                ||||+---- 1: Ignore mirroring control or above mirroring bit; instead provide four-screen VRAM
                ++++----- Lower nybble of mapper number
             */
            var flags6 = rom[6];
            bool hasTrainer = (flags6 & 4) == 4;

            /*
                76543210
                ||||||||
                |||||||+- VS Unisystem
                ||||||+-- PlayChoice-10 (8KB of Hint Screen data stored after CHR data)
                ||||++--- If equal to 2, flags 8-15 are in NES 2.0 format
                ++++----- Upper nybble of mapper number
             */
            var flags7 = rom[7];

            //Size of PRG RAM in 8 KB units (Value 0 infers 8 KB for compatibility;)
            var PRGRAMSize = rom[8] * 0x2000;

            var flags9 = rom[9];
            var flags10 = rom[10];

            PRGRomData = new byte[PRGROMSize];
            var prgStartIndex = 16 + (hasTrainer ? 512 : 0);
            Array.Copy(rom, prgStartIndex, PRGRomData, 0, PRGROMSize);

            CHRRomData = new byte[CHRROMSize];

            var chrStartIndex = prgStartIndex + PRGROMSize;
            Array.Copy(rom, chrStartIndex, CHRRomData, 0, CHRROMSize);
        }
    }
}