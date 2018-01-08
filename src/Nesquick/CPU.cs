using System;

namespace Nesquick
{
    public class CPU
    {
        public Memory Memory { get; private set; }

        #region Registers

        //Accumulator
        public byte _A { get; private set; }

        //Index X
        public byte _X { get; private set; }

        //Index y
        public byte _Y { get; private set; }

        //Program Counter
        public ushort _PC { get; set; }

        //Stack
        public byte _SP { get; private set; }

        //Status
        public byte _P
        {
            get
            {
                byte status = 0x0;

                if (CarryFlag)
                    status |= 0x1;

                if (ZeroFlag)
                    status |= 0x2;

                if (InterruptDisableFlag)
                    status |= 0x4;

                if (DecimalModeFlag)
                    status |= 0x8;

                if (BreakFlag)
                    status |= 0x10;

                //Bit 6: not used. Supposed to be logical 1 at all times.
                status |= 0x20;

                if (OverflowFlag)
                    status |= 0x40;

                if (NegativeFlag)
                    status |= 0x80;

                return status;
            }
            set
            {
                CarryFlag = (value & 0x1) == 0x1;
                ZeroFlag = (value & 0x2) == 0x2;
                InterruptDisableFlag = (value & 0x4) == 0x4;
                DecimalModeFlag = (value & 0x8) == 0x8;
                BreakFlag = (value & 0x10) == 0x10;
                OverflowFlag = (value & 0x40) == 0x40;
                NegativeFlag = (value & 0x80) == 0x80;
            }
        }

        #endregion Registers

        #region Flags

        public bool CarryFlag { get; private set; }
        public bool ZeroFlag { get; private set; }
        public bool InterruptDisableFlag { get; private set; }
        public bool DecimalModeFlag { get; private set; }
        public bool OverflowFlag { get; private set; }
        public bool NegativeFlag { get; private set; }
        public bool BreakFlag { get; private set; }

        #endregion

        public ushort Cycles { get; set; }

        public CPU() => Memory = new Memory();

        #region Instructions

        private void ADC(byte data)
        {
            var carry = CarryFlag ? 1 : 0;
            var result = _A + data + carry;

            //SetOverflowFlag((byte)((result ^ _A) & (result ^ data)));
            OverflowFlag = ((_A ^ result) & (data ^ result) & 128) != 0;

            _A = (byte)(result & 0xFF);

            CarryFlag = result > 255;
            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        private void ADC(ushort address) => ADC(Memory.Read8(address));

        [OpCode(Opcode = 0x69, Bytes = 2)]
        public void ADCImmediate() => ADC(Immediate());

        [OpCode(Opcode = 0x65, Bytes = 2, Cycles = 3)]
        public void ADCZeroPage() => ADC(ZeroPage());

        [OpCode(Opcode = 0x75, Bytes = 2, Cycles = 4)]
        public void ADCZeroPageX() => ADC(ZeroPageX());

        [OpCode(Opcode = 0x6D, Bytes = 3, Cycles = 4)]
        public void ADCAbsolute() => ADC(Absolute());

        [OpCode(Opcode = 0x7D, Bytes = 3, Cycles = 4)]
        public void ADCAbsoluteX() => ADC(AbsoluteY());

        [OpCode(Opcode = 0x79, Bytes = 3, Cycles = 4)]
        public void ADCAbsoluteY() => ADC(AbsoluteY());

        [OpCode(Opcode = 0x61, Bytes = 2, Cycles = 6)]
        public void ADCIndirectX() => ADC(IndirectX());

        [OpCode(Opcode = 0x71, Bytes = 2, Cycles = 5)]
        public void ADCIndirectY() => ADC(IndirectY());

        private void AND(byte data)
        {
            _A &= data;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        private void AND(ushort address) => AND(Memory.Read8(address));

        [OpCode(Opcode = 0x29, Bytes = 2, Cycles = 2)]
        public void ANDImmediate() => AND(Immediate());

        [OpCode(Opcode = 0x25, Bytes = 2, Cycles = 3)]
        public void ANDZeroPage() => AND(ZeroPage());

        [OpCode(Opcode = 0x35, Bytes = 2, Cycles = 4)]
        public void ANDZeroPageX() => AND(ZeroPageX());

        [OpCode(Opcode = 0x2D, Bytes = 3, Cycles = 4)]
        public void ANDAbsolute() => AND(Absolute());

        [OpCode(Opcode = 0x3D, Bytes = 3, Cycles = 4)]
        public void ANDAbsoluteX() => AND(AbsoluteY());

        [OpCode(Opcode = 0x39, Bytes = 3, Cycles = 4)]
        public void ANDAbsoluteY() => AND(AbsoluteY());

        [OpCode(Opcode = 0x21, Bytes = 2, Cycles = 6)]
        public void ANDIndirectX() => AND(IndirectX());

        [OpCode(Opcode = 0x31, Bytes = 2, Cycles = 5)]
        public void ANDIndirectY() => AND(IndirectY());

        public byte ASL(byte data)
        {
            CarryFlag = (data & 128) != 0;

            data <<= 1;

            SetZeroFlag(data);
            SetNegativeFlag(data);

            return data;
        }

        private byte ASL(ushort address) => ASL(Memory.Read8(address));

        [OpCode(Opcode = 0x0A)]
        public void ASLAccumulator() => _A = ASL(_A);

        [OpCode(Opcode = 0x06, Bytes = 2, Cycles = 5)]
        public void ASLZeroPage() => Memory.Write(ArgOne(), ASL(ZeroPage()));

        [OpCode(Opcode = 0x16, Bytes = 2, Cycles = 6)]
        public void ASLZeroPageX() => Memory.Write(ArgOne(), ASL(ZeroPageX()));

        [OpCode(Opcode = 0x0E, Bytes = 3, Cycles = 6)]
        public void ASLAbsolute() => Memory.Write(ArgOne16(), ASL(Absolute()));

        [OpCode(Opcode = 0x1E, Bytes = 3, Cycles = 7)]
        public void ASLAbsoluteX() => Memory.Write(ArgOne16(), ASL(AbsoluteX()));

        [OpCode(Opcode = 0x90, Bytes = 2, Cycles = 2)]
        public void BCC() => _PC += BranchIf(!CarryFlag, 2); //(ushort)(!CarryFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0xB0, Bytes = 2, Cycles = 2)]
        public void BCS() => _PC += BranchIf(CarryFlag, 2); //(ushort)(CarryFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0xF0, Bytes = 2, Cycles = 2)]
        public void BEQ() => _PC += BranchIf(ZeroFlag, 2); //_PC += (ushort)(ZeroFlag ? ArgOne() : 0);

        private ushort BranchIf(bool flag, ushort bytes) => (ushort)(flag ? ArgOne() : 0);

        private void BIT(ushort data)
        {
            var value = Memory.Read8(data);
            SetZeroFlag((byte)(value & _A));
            SetNegativeFlag(value);
            SetOverflowFlag(value);
        }

        [OpCode(Opcode = 0x24, Bytes = 2, Cycles = 3)]
        public void BITZeroPage() => BIT(ZeroPage());

        [OpCode(Opcode = 0x2C, Bytes = 3, Cycles = 4)]
        public void BITAbsolute() => BIT(Absolute());

        [OpCode(Opcode = 0x30, Bytes = 2, Cycles = 2)]
        public void BMI() => _PC += BranchIf(NegativeFlag, 2); //(ushort)(NegativeFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0xD0, Bytes = 2, Cycles = 2)]
        public void BNE() => _PC += BranchIf(!ZeroFlag, 2); //(ushort)(!ZeroFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0x10, Bytes = 2, Cycles = 2)]
        public void BPL() => _PC += BranchIf(!NegativeFlag, 2); //ushort)(!NegativeFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0x00, Cycles = 7)]
        public void BRK()
        {
            Push16(_PC);
            Push8(_P);

            _PC = Memory.Read16(0xFFFE);
            BreakFlag = true;
        }

        [OpCode(Opcode = 0x50, Bytes = 2, Cycles = 2)]
        public void BVC() => _PC += BranchIf(!OverflowFlag, 2); //(ushort)(!OverflowFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0x70, Bytes = 2, Cycles = 2)]
        public void BVS() => _PC += BranchIf(OverflowFlag, 2); //(ushort)(OverflowFlag ? ArgOne() : 0);

        [OpCode(Opcode = 0x18)]
        public void CLC() => CarryFlag = false;

        [OpCode(Opcode = 0xD8)]
        public void CLD() => DecimalModeFlag = false;

        [OpCode(Opcode = 0x58)]
        public void CLI() => InterruptDisableFlag = false;

        [OpCode(Opcode = 0xB8)]
        public void CLV() => OverflowFlag = false;

        private void CMP(byte data)
        {
            var result = _A - data;
            CarryFlag = _A >= data;
            ZeroFlag = _A == data;
            SetNegativeFlag((byte)result);
        }
        private void CMP(ushort address) => CMP(Memory.Read8(address));

        [OpCode(Opcode = 0xC9, Bytes = 2, Cycles = 2)]
        public void CMPImmediate() => CMP(Immediate());

        [OpCode(Opcode = 0xC5, Bytes = 2, Cycles = 3)]
        public void CMPZeroPage() => CMP(ZeroPage());

        [OpCode(Opcode = 0xD5, Bytes = 2, Cycles = 4)]
        public void CMPZeropPageX() => CMP(ZeroPageX());

        [OpCode(Opcode = 0xCD, Bytes = 3, Cycles = 4)]
        public void CMPAbsolute() => CMP(Absolute());

        [OpCode(Opcode = 0xDD, Bytes = 3, Cycles = 4)]
        public void CMPAbsoluteX() => CMP(AbsoluteX());

        [OpCode(Opcode = 0xD9, Bytes = 3, Cycles = 4)]
        public void CMPAbsoluteY() => CMP(AbsoluteY());

        [OpCode(Opcode = 0xC1, Bytes = 2, Cycles = 6)]
        public void CMPIndirectX() => CMP(IndirectX());

        [OpCode(Opcode = 0xD1, Bytes = 2, Cycles = 5)]
        public void CMPIndirectY() => CMP(IndirectY());

        private void CPX(byte data)
        {
            var result = _X - data;
            CarryFlag = _X >= data;
            ZeroFlag = _X == data;
            SetNegativeFlag((byte)result);
        }

        private void CPX(ushort address) => CPX(Memory.Read8(address));

        [OpCode(Opcode = 0xE0, Bytes = 2, Cycles = 2)]
        public void CPXImmediate() => CPX(Immediate());

        [OpCode(Opcode = 0xE4, Bytes = 2, Cycles = 3)]
        public void CPXZeroPage() => CPX(ZeroPage());

        [OpCode(Opcode = 0xEC, Bytes = 3, Cycles = 4)]
        public void CPXAbsolute() => CPX(Absolute());

        private void CPY(byte data)
        {
            var result = _Y - data;
            CarryFlag = _Y >= data;
            ZeroFlag = _Y == data;
            SetNegativeFlag((byte)result);
        }

        private void CPY(ushort address) => CPY(Memory.Read8(address));

        [OpCode(Opcode = 0xC0, Bytes = 2, Cycles = 2)]
        public void CPYImmediate() => CPY(Immediate());

        [OpCode(Opcode = 0xC4, Bytes = 2, Cycles = 3)]
        public void CPYZeroPage() => CPY(ZeroPage());

        [OpCode(Opcode = 0xCC, Bytes = 3, Cycles = 4)]
        public void CPYAbsolute() => CPY(Absolute());

        private void DEC(ushort address)
        {
            var result = (byte)(Memory.Read8(address) - 1);
            Memory.Write(address, result);

            SetZeroFlag(result);
            SetNegativeFlag(result);
        }

        [OpCode(Opcode = 0xC6, Bytes = 2, Cycles = 5)]
        public void DECZeroPage() => DEC(Immediate());

        [OpCode(Opcode = 0xD6, Bytes = 2, Cycles = 6)]
        public void DECZeroPageX() => DEC(ZeroPageX());

        [OpCode(Opcode = 0xCE, Bytes = 3, Cycles = 6)]
        public void DECAbsolute() => DEC(Absolute());

        [OpCode(Opcode = 0xDE, Bytes = 3, Cycles = 7)]
        public void DECAbsoluteX() => DEC(AbsoluteX());

        [OpCode(Opcode = 0xCA)]
        public void DEX()
        {
            _X--;

            SetZeroFlag(_X);
            SetNegativeFlag(_X);
        }

        [OpCode(Opcode = 0x88)]
        public void DEY()
        {
            _Y--;

            SetZeroFlag(_Y);
            SetNegativeFlag(_Y);
        }

        private void EOR(byte data)
        {
            _A ^= data;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        private void EOR(ushort address) => EOR(Memory.Read8(address));

        [OpCode(Opcode = 0x49, Bytes = 2, Cycles = 2)]
        public void EORImmediate() => EOR(Immediate());

        [OpCode(Opcode = 0x45, Bytes = 2, Cycles = 3)]
        public void EORZeroPage() => EOR(ZeroPage());

        [OpCode(Opcode = 0x55, Bytes = 2, Cycles = 4)]
        public void EORZeropPageX() => EOR(ZeroPageX());

        [OpCode(Opcode = 0x4D, Bytes = 3, Cycles = 4)]
        public void EORAbsolute() => EOR(Absolute());

        [OpCode(Opcode = 0x5D, Bytes = 3, Cycles = 4)]
        public void EORAbsoluteX() => EOR(AbsoluteX());

        [OpCode(Opcode = 0x59, Bytes = 3, Cycles = 4)]
        public void EORAbsoluteY() => EOR(AbsoluteY());

        [OpCode(Opcode = 0x41, Bytes = 2, Cycles = 6)]
        public void EORIndirectX() => EOR(IndirectX());

        [OpCode(Opcode = 0x51, Bytes = 2, Cycles = 5)]
        public void EORIndirectY() => EOR(IndirectY());
        private void INC(ushort address)
        {
            var result = (byte)(Memory.Read8(address) + 1);
            Memory.Write(address, result);

            SetZeroFlag(result);
            SetNegativeFlag(result);
        }

        [OpCode(Opcode = 0xE6, Bytes = 2, Cycles = 5)]
        public void INCZeroPage() => INC(ZeroPage());

        [OpCode(Opcode = 0xF6, Bytes = 2, Cycles = 6)]
        public void INCZeroPageX() => INC(ZeroPageX());

        [OpCode(Opcode = 0xEE, Bytes = 3, Cycles = 6)]
        public void INCAbsolute() => INC(Absolute());

        [OpCode(Opcode = 0xFE, Bytes = 3, Cycles = 7)]
        public void INCAbsoluteX() => INC(AbsoluteX());

        [OpCode(Opcode = 0xE8)]
        public void INX()
        {
            _X++;

            SetZeroFlag(_X);
            SetNegativeFlag(_X);
        }

        [OpCode(Opcode = 0xC8)]
        public void INY()
        {
            _Y++;

            SetZeroFlag(_Y);
            SetNegativeFlag(_Y);
        }

        private void JMP(ushort address) => _PC = address;

        [OpCode(Opcode = 0x4C, Bytes = 3, Cycles = 3, IncrementPC = false)]
        public void JMPAbsolute() => JMP(Absolute());

        [OpCode(Opcode = 0x6C, Bytes = 3, Cycles = 5, IncrementPC = false)]
        public void JMPIndirect() => JMP(Indirect());

        [OpCode(Opcode = 0x20, Bytes = 3, Cycles = 6, IncrementPC = false)]
        public void JSR()
        {
            Push16((ushort)(_PC + 2));

            JMPAbsolute();
        }
        public void LDA(byte data)
        {
            _A = data;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        private void LDA(ushort address) => LDA(Memory.Read8(address));

        [OpCode(Opcode = 0xA9, Bytes = 2)]
        public void LDAImmediate() => LDA(Immediate());

        [OpCode(Opcode = 0xA5, Bytes = 2, Cycles = 3)]
        public void LDAZeroPage() => LDA(ZeroPage());

        [OpCode(Opcode = 0xB5, Bytes = 2, Cycles = 4)]
        public void LDAZeroPageX() => LDA(ZeroPageX());

        [OpCode(Opcode = 0xAD, Bytes = 3, Cycles = 4)]
        public void LDAAbsolute() => LDA(Absolute());

        [OpCode(Opcode = 0xBD, Bytes = 3, Cycles = 4)]
        public void LDAAbsoluteX() => LDA(AbsoluteX());

        [OpCode(Opcode = 0xB9, Bytes = 3, Cycles = 4)]
        public void LDAAbsoluteY() => LDA(AbsoluteY());

        [OpCode(Opcode = 0xA1, Bytes = 2, Cycles = 6)]
        public void LDAIndirectX() => LDA(IndirectX());

        [OpCode(Opcode = 0xB1, Bytes = 2, Cycles = 5)]
        public void LDAIndirectY() => LDA(IndirectY());

        public void LDX(byte data)
        {
            _X = data;

            SetZeroFlag(_X);
            SetNegativeFlag(_X);
        }

        private void LDX(ushort address) => LDX(Memory.Read8(address));

        [OpCode(Opcode = 0xA2, Bytes = 2)]
        public void LDXImmediate() => LDX(Immediate());

        [OpCode(Opcode = 0xA6, Bytes = 2, Cycles = 3)]
        public void LDXZeroPage() => LDX(ZeroPage());

        [OpCode(Opcode = 0xB6, Bytes = 2, Cycles = 4)]
        public void LDXZeroPageY() => LDX(ZeroPageY());

        [OpCode(Opcode = 0xAE, Bytes = 3, Cycles = 4)]
        public void LDXAbsolute() => LDX(Absolute());

        [OpCode(Opcode = 0xBE, Bytes = 3, Cycles = 4)]
        public void LDXAbsoluteY() => LDX(AbsoluteY());

        public void LDY(byte data)
        {
            _Y = data;

            SetZeroFlag(_Y);
            SetNegativeFlag(_Y);
        }

        private void LDY(ushort address) => LDY(Memory.Read8(address));

        [OpCode(Opcode = 0xA0, Bytes = 2)]
        public void LDYImmediate() => LDY(Immediate());

        [OpCode(Opcode = 0xA4, Bytes = 2, Cycles = 3)]
        public void LDYZeroPage() => LDY(ZeroPage());

        [OpCode(Opcode = 0xB4, Bytes = 2, Cycles = 4)]
        public void LDYZeroPageX() => LDY(ZeroPageX());

        [OpCode(Opcode = 0xAC, Bytes = 3, Cycles = 4)]
        public void LDYAbsolute() => LDY(Absolute());

        [OpCode(Opcode = 0xBC, Bytes = 3, Cycles = 4)]
        public void LDYAbsoluteX() => LDY(AbsoluteX());

        public byte LSR(byte data)
        {
            CarryFlag = (data & 1) == 1;

            data >>= 1;

            SetZeroFlag(data);
            SetNegativeFlag(data);

            return data;
        }

        private byte LSR(ushort address) => LSR(Memory.Read8(address));

        [OpCode(Opcode = 0x4A)]
        public void LSRAccumulator() => _A = LSR(_A);

        [OpCode(Opcode = 0x46, Bytes = 2, Cycles = 5)]
        public void LSRZeroPage() => Memory.Write(ArgOne(), LSR(ZeroPage()));

        [OpCode(Opcode = 0x56, Bytes = 2, Cycles = 6)]
        public void LSRZeroPageX() => Memory.Write(ArgOne(), LSR(ZeroPageX()));

        [OpCode(Opcode = 0x4E, Bytes = 3, Cycles = 6)]
        public void LSRAbsolute() => Memory.Write(ArgOne16(), LSR(Absolute()));

        [OpCode(Opcode = 0x5E, Bytes = 3, Cycles = 7)]
        public void LSRAbsoluteX() => Memory.Write(ArgOne16(), LSR(AbsoluteX()));

        [OpCode(Opcode = 0xEA, Bytes = 1, IncrementPC = false)]
        public void NOP() => _PC++;

        private void ORA(byte data)
        {
            _A |= data;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }
        private void ORA(ushort address) => ORA(Memory.Read8(address));

        [OpCode(Opcode = 0x09, Bytes = 2)]
        public void ORAImmediate() => ORA(Immediate());

        [OpCode(Opcode = 0x05, Bytes = 2, Cycles = 3)]
        public void ORAZeroPage() => ORA(ZeroPage());

        [OpCode(Opcode = 0x15, Bytes = 2, Cycles = 4)]
        public void ORAZeroPageX() => ORA(ZeroPageX());

        [OpCode(Opcode = 0x0D, Bytes = 3, Cycles = 4)]
        public void ORAAbsolute() => ORA(Absolute());

        [OpCode(Opcode = 0x1D, Bytes = 3, Cycles = 4)]
        public void ORAAbsoluteX() => ORA(AbsoluteX());

        [OpCode(Opcode = 0x19, Bytes = 3, Cycles = 4)]
        public void ORAAbsoluteY() => ORA(AbsoluteY());

        [OpCode(Opcode = 0x01, Bytes = 2, Cycles = 6)]
        public void ORAIndirectX() => ORA(IndirectX());

        [OpCode(Opcode = 0x11, Bytes = 2, Cycles = 5)]
        public void ORAIndirectY() => ORA(IndirectY());

        [OpCode(Opcode = 0x48, Cycles = 3)]
        public void PHA() => Push8(_A);

        [OpCode(Opcode = 0x08, Cycles = 3)]
        public void PHP() => Push8((byte)(_P | 0x10));

        [OpCode(Opcode = 0x68, Cycles = 4)]
        public void PLA()
        {
            _A = Pop8();

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        [OpCode(Opcode = 0x28, Cycles = 4)]
        public void PLP() => _P = PopProcessorStatus();

        private void RLA(byte data)
        {
            ROL(data);
            AND(data);
        }
        private void RLA(ushort address) => RLA(Memory.Read8(address));

        [OpCode(Opcode = 0x27, Cycles = 5)]
        public void RLAZeroPage() => RLA(ZeroPage());

        [OpCode(Opcode = 0x37, Cycles = 6)]
        public void RLAZeroPageX() => RLA(ZeroPageX());

        [OpCode(Opcode = 0x2F, Cycles = 6)]
        public void RLAAbsolute() => RLA(Absolute());

        [OpCode(Opcode = 0x3F, Cycles = 7)]
        public void RLAAbsoluteX() => RLA(AbsoluteX());

        [OpCode(Opcode = 0x3B, Cycles = 7)]
        public void RLAAbsoluteY() => RLA(AbsoluteY());

        [OpCode(Opcode = 0x23, Cycles = 8)]
        public void RLAIndirectX() => RLA(IndirectX());

        [OpCode(Opcode = 0x33, Cycles = 8)]
        public void RLAIndirectY() => RLA(IndirectY());

        private byte ROL(byte data)
        {
            var currentCflag = (byte)(CarryFlag ? 1 : 0);

            SetCarryFlag(data);

            data = (byte)((data << 1) | currentCflag);

            SetZeroFlag(data);
            SetNegativeFlag(data);

            return data;
        }

        private byte ROL(ushort address) => ROL(Memory.Read8(address));

        [OpCode(Opcode = 0x2A)]
        public void ROLAccumulator() => _A = ROL(_A);

        [OpCode(Opcode = 0x26, Bytes = 2, Cycles = 5)]
        public void ROLZeroPage() => Memory.Write(ArgOne(), ROL(ZeroPage()));

        [OpCode(Opcode = 0x36, Bytes = 2, Cycles = 6)]
        public void ROLZeroPageX() => Memory.Write(ArgOne(), ROL(ZeroPageX()));

        [OpCode(Opcode = 0x2E, Bytes = 3, Cycles = 6)]
        public void ROLAbsolute() => Memory.Write(ArgOne16(), ROL(Absolute()));

        [OpCode(Opcode = 0x3E, Bytes = 3, Cycles = 7)]
        public void ROLAbsoluteX() => Memory.Write(ArgOne16(), ROL(AbsoluteX()));

        private byte ROR(byte data)
        {
            var oldCarryFlag = CarryFlag;

            CarryFlag = (data & 1) == 1;

            data = (byte)((data >> 1) | (oldCarryFlag ? 128 : 0));

            SetZeroFlag(data);
            SetNegativeFlag(data);

            return data;
        }

        private byte ROR(ushort address) => ROR(Memory.Read8(address));

        [OpCode(Opcode = 0x6A)]
        public void RORAccumulator() => _A = ROR(_A);

        [OpCode(Opcode = 0x66, Bytes = 2, Cycles = 5)]
        public void RORZeroPage() => Memory.Write(ArgOne(), ROR(ZeroPage()));

        [OpCode(Opcode = 0x76, Bytes = 2, Cycles = 6)]
        public void RORZeroPageX() => Memory.Write(ArgOne(), ROR(ZeroPageX()));

        [OpCode(Opcode = 0x6E, Bytes = 3, Cycles = 6)]
        public void RORAbsolute() => Memory.Write(ArgOne16(), ROR(Absolute()));

        [OpCode(Opcode = 0x7E, Bytes = 3, Cycles = 7)]
        public void RORAbsoluteX() => Memory.Write(ArgOne16(), ROR(AbsoluteX()));

        [OpCode(Opcode = 0x40, Cycles = 6, IncrementPC = false)]
        public void RTI()
        {
            _P = PopProcessorStatus();
            _PC = Pop16();
        }

        [OpCode(Opcode = 0x60, Bytes = 0, Cycles = 6)]
        public void RTS() => _PC = (ushort)(Pop16() + 1);

        private void SBC(byte data) => ADC((byte)(data ^ 0xFF)); //Inverse of ADC, right?

        private void SBC(ushort address) => SBC(Memory.Read8(address));

        [OpCode(Opcode = 0xE9, Bytes = 2)]
        public void SBCImmediate() => SBC(Immediate());

        [OpCode(Opcode = 0xE5, Bytes = 2, Cycles = 3)]
        public void SBCZeroPage() => SBC(ZeroPage());

        [OpCode(Opcode = 0xF5, Bytes = 2, Cycles = 4)]
        public void SBCZeroPageX() => SBC(ZeroPageX());

        [OpCode(Opcode = 0xED, Bytes = 3, Cycles = 4)]
        public void SBCAbsolute() => SBC(Absolute());

        [OpCode(Opcode = 0xFD, Bytes = 3, Cycles = 4)]
        public void SBCAbsoluteX() => SBC(AbsoluteX());

        [OpCode(Opcode = 0xF9, Bytes = 3, Cycles = 4)]
        public void SBCAbsoluteY() => SBC(AbsoluteY());

        [OpCode(Opcode = 0xE1, Bytes = 2, Cycles = 6)]
        public void SBCIndirectX() => SBC(IndirectX());

        [OpCode(Opcode = 0xF1, Bytes = 2, Cycles = 5)]
        public void SBCIndirectY() => SBC(IndirectY());

        [OpCode(Opcode = 0x38)]
        public void SEC() => CarryFlag = true;

        [OpCode(Opcode = 0xF8)]
        public void SED() => DecimalModeFlag = true;

        [OpCode(Opcode = 0x78)]
        public void SEI() => InterruptDisableFlag = true;

        private void STA(ushort address) => Memory.Write(address, _A);

        [OpCode(Opcode = 0x85, Bytes = 2, Cycles = 3)]
        public void STAZeroPage() => STA(ZeroPage());

        [OpCode(Opcode = 0x95, Bytes = 2, Cycles = 4)]
        public void STAZeroPageX() => STA(ZeroPageX());

        [OpCode(Opcode = 0x8D, Bytes = 3, Cycles = 4)]
        public void STAAbsolute() => STA(Absolute());

        [OpCode(Opcode = 0x9D, Bytes = 3, Cycles = 5)]
        public void STAAbsoluteX() => STA(AbsoluteX());

        [OpCode(Opcode = 0x99, Bytes = 3, Cycles = 5)]
        public void STAAbsoluteY() => STA(AbsoluteY());

        [OpCode(Opcode = 0x81, Bytes = 2, Cycles = 6)]
        public void STAIndirectX() => STA(IndirectX());

        [OpCode(Opcode = 0x91, Bytes = 2, Cycles = 6)]
        public void STAIndirectY() => STA(IndirectY());
        private void STX(ushort address) => Memory.Write(address, _X);

        [OpCode(Opcode = 0x86, Bytes = 2, Cycles = 3)]
        public void STXZeroPage() => STX(ZeroPage());

        [OpCode(Opcode = 0x96, Bytes = 2, Cycles = 4)]
        public void STXZeroPageY() => STX(ZeroPageY());

        [OpCode(Opcode = 0x8E, Bytes = 3, Cycles = 4)]
        public void STXAbsolute() => STX(Absolute());

        private void STY(ushort address) => Memory.Write(address, _Y);

        [OpCode(Opcode = 0x84, Bytes = 2, Cycles = 3)]
        public void STYZeroPage() => STY(ZeroPage());

        [OpCode(Opcode = 0x94, Bytes = 2, Cycles = 4)]
        public void STYZeroPageX() => STY(ZeroPageX());

        [OpCode(Opcode = 0x8C, Bytes = 3, Cycles = 4)]
        public void STYAbsolute() => STY(Absolute());

        [OpCode(Opcode = 0xAA)]
        public void TAX()
        {
            _X = _A;

            SetZeroFlag(_X);
            SetNegativeFlag(_X);
        }

        [OpCode(Opcode = 0xA8)]
        public void TAY()
        {
            _Y = _A;

            SetZeroFlag(_Y);
            SetNegativeFlag(_Y);
        }

        [OpCode(Opcode = 0xBA)]
        public void TSX()
        {
            _X = _SP;

            SetZeroFlag(_X);
            SetNegativeFlag(_X);
        }

        [OpCode(Opcode = 0x8A)]
        public void TXA()
        {
            _A = _X;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        [OpCode(Opcode = 0x9A)]
        public void TXS() => _SP = _X;

        [OpCode(Opcode = 0x98)]
        public void TYA()
        {
            _A = _Y;

            SetZeroFlag(_A);
            SetNegativeFlag(_A);
        }

        #endregion

        public void Reset()
        {
            CarryFlag = false;
            ZeroFlag = false;
            InterruptDisableFlag = true;
            DecimalModeFlag = false;
            BreakFlag = true;
            OverflowFlag = false;
            NegativeFlag = false;

            _A = _X = _Y = 0;
            _SP = 0xFD;
        }

        private void SetCarryFlag(byte value) => CarryFlag = (value >> 7) == 1;

        private void SetZeroFlag(byte value) => ZeroFlag = value == 0;

        private void SetNegativeFlag(byte value) => NegativeFlag = value > 0x7f;

        private void SetOverflowFlag(byte value) => OverflowFlag = (value & 0x40) != 0;

        private void Push8(byte data)
        {
            Memory.Write((ushort)(0x0100 + _SP), data);
            _SP--;
        }
        private void Push16(ushort data)
        {
            Push8((byte)((data & 0xFF00) >> 8));
            Push8((byte)(data & 0xFF));
        }
        private byte Pop8()
        {
            _SP++;
            return Memory.Read8((ushort)(0x0100 + _SP));
        }

        private ushort Pop16()
        {
            _SP += 2;
            return Memory.Read16((ushort)((0x0100 + (byte)(_SP - 1))));
        }

        private byte PopProcessorStatus() => (byte)(Pop8() & ~(1 << 4)); //ensure that break flag is not set

        private byte ArgOne() => Memory.Read8((ushort)(_PC + 1));

        private ushort ArgOne16() => Memory.Read16((ushort)(_PC + 1));

        private byte Immediate() => ArgOne();

        private ushort ZeroPage() => ArgOne();

        private ushort ZeroPageX() => (ushort)(ArgOne() + _X);

        private ushort ZeroPageY() => (ushort)(ArgOne() + _Y);

        private ushort Absolute() => ArgOne16();

        private ushort AbsoluteX() => (ushort)(ArgOne16() + _X);

        private ushort AbsoluteY() => (ushort)(ArgOne16() + _Y);

        private ushort Indirect() => Memory.Read16(ArgOne16(), true);

        private ushort IndirectX() => Memory.Read16((ushort)((ArgOne() + _X) & 0xFF), true);

        private ushort IndirectY() => (ushort)((Memory.Read16(ArgOne(), true)) + _Y);

    }
}