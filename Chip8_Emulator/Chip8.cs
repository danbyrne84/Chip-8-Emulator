using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Chip8
    {
        /// <summary>
        /// Draw flag is set whenever OpCode 0x00E0 or 0xDXYN is used (draw sprite or clear screen)
        /// </summary>
        protected Boolean drawflag = false;

        public Boolean DrawFlag
        {
            get
            {
                return drawflag;
            }
        }

        /// <summary>
        /// Memory
        /// </summary>
        protected byte[] memory = new byte[4096];

        /// <summary>
        /// Current opcode
        /// </summary>
        protected int opcode;

        /// <summary>
        /// CPU registers starting from V0 to VE. 16th register is carry flag.
        /// </summary>
        protected char[] v = new char[16];

        /// <summary>
        /// Index register
        /// </summary>
        protected int i;

        /// <summary>
        /// Program counter
        /// </summary>
        protected int pc;

        /// <summary>
        /// Graphics array contains grid of pixels 64*32
        /// </summary>
        protected char[,] gfx = new char[64, 32];

        /// <summary>
        /// Delay timer
        /// </summary>
        protected char delayTimer;

        /// <summary>
        /// Sound timer
        /// </summary>
        protected char soundTimer;

        /// <summary>
        /// The stack
        /// </summary>
        protected int[] stack = new int[16];

        /// <summary>
        /// The stack pointer
        /// </summary>
        protected int stackPointer;

        /// <summary>
        /// Current state of the keypad (0x0 - 0xF)
        /// </summary>
        protected char[] key = new char[16];

        /// <summary>
        /// Constructor
        /// </summary>
        public Chip8()
        {
            // lets plant some instructions into the chip 8 memory
            this.memory[0x200] = 0xA2;
            this.memory[0x201] = 0xF0;

            this.i = 0;
            this.stackPointer = 0;

            // set the program counter to 0x200 (start of working RAM)
            this.pc = 0x200;
        }

        /// <summary>
        /// Emulate a cycle of the Chip8 CPU
        /// </summary>
        public void EmulateCycle()
        {
            // Fetch Opcode
            this.FetchOpcode();
            
            // Decode Opcode
            this.ExecuteOpcode();

            // Update Timers
        }

        protected void ExecuteOpcode()
        {
            int opCode = this.opcode;

            switch (opCode)
            {
                case 0x00E0:
                    // clear the screen, just reset the array of pixels
                    this.gfx = new char[64, 32];
                    break;
                case 0x0:
                    // return from subroutine call
                    break;
                case 0x1:
                    // jump to address 
                    break;
                case 0x2:
                    // call a subroutine
                    break;
                case 0x3:
                    // skip if equal
                    break;
                case 0x4:
                    // skip if not equal
                    break;
                case 0x5:
                    // skip if register is equal
                    break;
                case 0x6:
                    // load value
                    break;
                case 0x7:
                    // add value
                    break;
                case 0x8:
                    // math.... to be expanded
                    break;
                case 0x9:
                    // skip if register not equal
                    break;
                case 0xA:
                    // load value into index register
                    break;
                case 0xB:
                    // jump to address plus register 0
                    break;
                case 0xC:
                    // random (register VX = random number AND KK)
                    break;
                case 0xD:
                    // draw sprite
                    break;
                case 0xE:
                    // skip if key is pressed/not pressedd
                    break;
                case 0xF:
                    // timing... and stuff...
                    break;
                default:
                    throw new Exception("Opcode " + opCode + " not recognised");
            }
        }

        /// <summary>
        /// Fetch an OpCode from the memory
        /// </summary>
        /// <returns></returns>
        protected void FetchOpcode()
        {
            // combine 2 bytes beginning with PC location to get the OpCode
            int op1 = this.memory[this.pc] << 8;
            int op2 = this.memory[this.pc + 1];

            this.opcode = op1 | op2;
        }

        /// <summary>
        /// Loads a ROM file into the memory bytearray
        /// </summary>
        /// <param name="romFile">path to the ROM file</param>
        /// <returns></returns>
        public Boolean LoadRom(System.IO.FileInfo romFile)
        {
            System.IO.FileStream fs = romFile.OpenRead();

            int offset = 0;
            int remaining = (int)fs.Length;

            while(remaining > 0)
            {
                int read = fs.Read(this.memory, offset, remaining);
                remaining -= read;
                offset += read;
            }

            return true;
        }
    }
}
