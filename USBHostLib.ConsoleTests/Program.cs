using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using USBHostLib;

namespace USBHostLib.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                HIDFinder finder = new HIDFinder();
                IHIDDevice deviceFound = finder.FindDevice(0x8d8, 0x101);

                if (deviceFound == null)
                    Console.WriteLine("Device not found...");
                else
                {
                    Console.WriteLine("Device Found!");

                    DateTime startTime = DateTime.Now;
                    for (int i = 0; i < 4; i++)
                    {
                        bool written = deviceFound.WriteReportViaInterruptTransfer(new byte[] { 0x37, 00 });
                        Console.WriteLine("Data written: " + written);
                        byte[] dataRead = deviceFound.ReadReportViaInterruptTransfer();
                        int value = dataRead[1] | dataRead[2] << 8;
                        Console.WriteLine("Data Read: " + value);
                    }
                    DateTime endTime = DateTime.Now;

                    Console.WriteLine("Time consumed: " + (endTime - startTime));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            Console.ReadKey();
        }
    }
}
