using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

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
        protected bool[,] gfx = new bool[64, 32];

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
            this.i = 0;
            this.stackPointer = 0;

            // set the program counter to 0x200 (start of working RAM)
            this.pc = 0x200;
        }

        /// <summary>
        /// Emulate a cycle of the Chip8 CPU
        /// Fetch, Execute, Update Timers, Update Display
        /// </summary>
        public void EmulateCycle()
        {
            // Fetch Opcode
            this.FetchOpcode();
            
            // Decode Opcode
            this.ExecuteOpcode();

            // Update Timers

            // Update Display
            this.UpdateDisplay();

            // Just slow down a little sparky
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// Execute the current OpCode
        /// Contains switch statement of doom, and poor treatment of hexadecimals!
        /// </summary>
        protected void ExecuteOpcode()
        {
            int opCode = this.opcode;

            string hex = opCode.ToString("X");
            Debug.Print("Executing opcode: " + hex);

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

                    int xPos = this.v[x];
                    int yPos = this.v[y];

                    int nibble = int.Parse(hex[3].ToString(), System.Globalization.NumberStyles.HexNumber);
                    this.drawSprite(xPos, yPos, nibble);
                break;
                case 'E':
                    // skip if key is pressed/not pressed
                break;
                

                /**
                case '0x00E0': // @todo moves
                    // clear the screen, just reset the array of pixels
                    this.gfx = new char[64, 32];
                    break;
                 **/
                case '2':
                    // call a subroutine
                    this.stackPointer++;
                    this.stack.Add(this.pc);
                    this.pc = int.Parse(String.Concat(hex[1], hex[2], hex[3]), System.Globalization.NumberStyles.HexNumber);
                break;
                case '7':
                    // add value
                    this.addValue(int.Parse((String)hex[1].ToString(), System.Globalization.NumberStyles.HexNumber), int.Parse(String.Concat(hex[2], hex[3]), System.Globalization.NumberStyles.HexNumber));
                break;

                case '0':
                // return from subroutine call
                case '1':
                    // jump to address 
                    this.pc = int.Parse(String.Concat(hex[1], hex[2], hex[3]), System.Globalization.NumberStyles.HexNumber);
                    break;
                case '3':
                // skip if equal
                case '4':
                // skip if not equal
                case '5':
                // skip if register is equal
                case '8':
                    // 8XY4
                    // ADD Value of register Y to value of register X 
                case '9':
                    // skip if register not equal
                case 'B':
                    // jump to address plus register 0
                case 'C':
                    // random (register VX = random number AND KK)
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

        /// <summary>
        /// Add value inVal to Register
        /// Value wraps around if greater than 255
        /// </summary>
        /// <param name="Register"></param>
        /// <param name="inVal"></param>
        protected void addValue(int Register, int inVal)
        {
            int val;

            if ((val = this.v[Register] + inVal) > 255)
            {
                val -= 256;
            }
            this.v[Register] = val;

            Debug.WriteLine("ADDV [" + Register + "] with [" + inVal + "] = [" + val + "]");
        }

        /// <summary>
        /// Load a value into the given register
        /// </summary>
        /// <param name="Register"></param>
        /// <param name="value"></param>
        protected void loadValue(int Register, int value)
        {
            Debug.Print("LDV [" + Register + "] with [" + value + "]");
            this.v[Register] = value;
        }

        /// <summary>
        /// Load the index register with the given value
        /// </summary>
        /// <param name="value"></param>
        protected void loadIndex(int value)
        {
            Debug.Print("LDI [" + value + "]");
            this.i = value;
        }

        /// <summary>
        /// Draw a sprite to the 'screen' (graphics array)
        /// </summary>
        /// <param name="x">X co-ordinate</param>
        /// <param name="y">Y co-ordinate</param>
        /// <param name="nibble">pixel height of the sprite</param>
        protected void drawSprite(int x, int y, int nibble)
        {
            Debug.WriteLine("X Coordinate: [{0}] , Y Coordinate: [{1}]", x, y);
 
            for (int lines = 0; lines < nibble; lines++)
            {
                Byte sprite = this.memory[this.i + lines];
                String byteString = Convert.ToString(sprite, 2).PadLeft(8, '0');

                // for each bit in the byte
                for (int i = 0; i < 8; i++)
                {
                    bool bit = (byteString[i] == '1') ? true : false;
                    bool existingVal = this.gfx[x + i, y + lines];

                    //XOR operator to detect 'collisions'
                    this.gfx[x + i, y + lines] = existingVal ^ bit;
                }
            }           
        }

        /// <summary>
        /// This is one ugly ass method
        /// Update the console with pixels from the gfx array
        /// </summary>
        protected void UpdateDisplay()
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(1252);
            System.Console.Clear();
            char block = '*';

            for (int xPos = 0; xPos < 64; xPos++)
            {
                Console.Write(block);
            }
            Console.WriteLine();

            for (int yPos = 0; yPos < 32; yPos++)
            {
                for(int xPos = 0; xPos < 64; xPos++)
                {
                    if (xPos == 0 || xPos == 63)
                    {
                        Console.Write(block);
                    }

                    char val = (this.gfx[xPos, yPos] == true) ? block : ' ';
                    System.Console.Write(val);
                }
                Console.WriteLine();
            }

            for (int xPos = 0; xPos < 64; xPos++)
            {
                Console.Write(block);
            }
            Console.WriteLine();
        }

    }
}
