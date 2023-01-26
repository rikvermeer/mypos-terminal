// See https://aka.ms/new-console-template for more information
using System;
using myPOS;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace myPOS
{
    class Program
    {
        protected static void ProcessResult(ProcessingResult r)
       {
        Console.WriteLine("ProcessingResult r:", r.ToString());
        Console.WriteLine("result status:", r.Status);
	       // handle the response here
            if (r.TranData != null)
            {
		          // transaction response
                  Console.WriteLine("result data:", r.TranData);
            }
 	}

        private static SerialPort m_port;
        static void Main(string[] args)
        {
            

            // string m_portName = "/dev/ttyPos0";
            // int m_baudRate = 115200;
            // Parity m_parityBit = Parity.None;
            // int m_dataBits = 8;
            // StopBits m_stopBit = StopBits.One;

            // m_port = new SerialPort
            // {
            //     PortName = m_portName,
            //     BaudRate = m_baudRate,
            //     Parity = m_parityBit,
            //     DataBits = m_dataBits,
            //     StopBits = m_stopBit
            // };

            // m_port.Open();

//new Field("PROTOCOL", "IPP"),
//new Field("VERSION", "200"),
//new Field("METHOD", method),
//new Field("SID", Guid.NewGuid().ToString())
/*
PROTOCOL=IPP
VERSION=200
METHOD=GET_STATUS
SID=1
*/

            Console.WriteLine("Hello, Wosssrld!");
            myPOSTerminal terminal = new myPOSTerminal();
            terminal.ProcessingFinished += ProcessResult;
            //RequestResult result = terminal.Initialize("/dev/ttyPos0");
            RequestResult result = terminal.Initialize("/dev/ttyUSB0");
            Console.WriteLine("result:" + result);
            
            Thread.Sleep(5000);

            //Console.Write(terminal.GetStatus());
        }
    }
}