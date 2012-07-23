using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Chip8 chip8 = new Chip8();

            // load the pong ROM file into the emulator
            System.IO.FileInfo finf = new System.IO.FileInfo(System.IO.Directory.GetCurrentDirectory() + "/roms/IBM Logo.ch8");
            chip8.LoadRom(finf);

            for (; ; )
            {
                chip8.EmulateCycle();

                if (chip8.DrawFlag == true)
                {
                    // call graphics drawing routine
                }
            }

        }
    }
}
