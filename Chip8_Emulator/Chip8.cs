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
        protected int[] v = new int[16];

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
        protected byte[,] gfx = new byte[64, 32];

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
        protected List<int> stack = new List<int>();

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
            // this.memory[0x200] = 0xA2;
            // this.memory[0x201] = 0xF0;

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

            if (opCode == 0)
            {
                Console.WriteLine("END");
            }

            string hex = opCode.ToString("X");
            Console.WriteLine("Executing opcode: " + hex);
            char first = hex[0];

            int register;
            int value;

            switch (first)
            {
                case '6': // load value into register
                    register = int.Parse(hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
                    value = int.Parse(String.Concat(hex[2],hex[3]), System.Globalization.NumberStyles.HexNumber);
                    this.loadValue(register, value);
                break;
                case 'A': // load value into index register
                    value = int.Parse(String.Concat(hex[1],hex[2],hex[3]), System.Globalization.NumberStyles.HexNumber);
                    this.loadIndex(value);
                break;
                case 'D': // draw sprite
                    int x = int.Parse(hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
                    int y = int.Parse(hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
                    int nibble = int.Parse(hex[3].ToString(), System.Globalization.NumberStyles.HexNumber);
                    this.drawSprite(x, y, nibble);
                break;

                /**
                case '0x00E0': // @todo moves
                    // clear the screen, just reset the array of pixels
                    this.gfx = new char[64, 32];
                    break;
                 **/
                case '0':
                // return from subroutine call
                case '1':
                // jump to address 
                    break;
                case '2':
                // call a subroutine
                    this.stackPointer++;
                    this.stack.Add(this.pc);
                    this.pc = int.Parse(String.Concat(hex[1], hex[2], hex[3]), System.Globalization.NumberStyles.HexNumber);
                break;
                case '3':
                // skip if equal
                case '4':
                // skip if not equal
                case '5':
                // skip if register is equal

                case '7':
                    // add value
                case '8':
                    // math.... to be expanded
                case '9':
                    // skip if register not equal
                case 'B':
                    // jump to address plus register 0
                case 'C':
                    // random (register VX = random number AND KK)
                case 'E':
                    // skip if key is pressed/not pressedd
                case 'F':
                    // timing... and stuff...
                default:
                    throw new Exception("Opcode " + hex + " not recognised");
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

            this.pc = this.pc + 2;
            
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

            int offset = 512; // 0x200 - start of working RAM
            int remaining = (int)fs.Length;

            while (remaining > 0)
            {
                int read = fs.Read(this.memory, offset, remaining);
                remaining -= read;
                offset += read;
            }

            return true;
        }

        /**
         * Load a valid into the given register
         */
        protected void loadValue(int Register, int value)
        {
            this.v[Register] = value;
        }

        protected void loadIndex(int value)
        {
            this.i = value;
        }

        protected void drawSprite(int x, int y, int nibble)
        {
            // read nibble bytes from memory starting at location stored in index register
            ArraySegment<byte> sprite = new ArraySegment<byte>(this.memory, this.i, nibble);
            
            // copy the array segment into the graphics buffer

        }

    }
}
