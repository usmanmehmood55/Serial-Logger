using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serial_Logger
{
    internal class Program
    {
        private static SerialPort port = new SerialPort();
        private static List<string> rawData = new List<string>();

        public static void Main(string[] args)
        {
            PrintIntro();
            string[] portNames = SerialPort.GetPortNames();
            if (portNames.Length > 0)
            {
                port.PortName = "COM" + SelectPortNumber(portNames);
                port.BaudRate = SelectBaudRate();
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;

                int recordingDuration = SelectRecordingDuration();

                port.Open();
                if (port.IsOpen) Console.WriteLine("\nSerial read init");
                Task t = Task.Run(() => { ReadSerial(); });
                TimeSpan ts = TimeSpan.FromMilliseconds(recordingDuration);

                if (!t.Wait(ts))
                {
                    port.Close();
                    WriteToFile();
                    Console.WriteLine("Press any key to close");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("No serial ports found");
                Console.WriteLine("Press any key to close");
                Console.ReadLine();
            }
        }

        private static int SelectRecordingDuration()
        {
            int recordingDuration = 0;
            bool isSelected = false;
            while (!isSelected)
            {
                Console.WriteLine("\nEnter recording duration");
                Console.WriteLine("\"s\" post-fix for seconds and \"ms\" post-fix for milliseconds");
                Console.WriteLine("If there is no post-fix, milliseconds will be assumed");
                Console.WriteLine("If there is both, seconds will be assumed");

                string durationString = Console.ReadLine();
                string flteredInp = Regex.Match(durationString, @"[0-9]+").Value;

                isSelected = int.TryParse(flteredInp, out recordingDuration);

                if (durationString.Contains("ms"))
                {
                    isSelected = int.TryParse(flteredInp, out recordingDuration);
                    Console.WriteLine($"{recordingDuration} milliseconds");
                }
                else if (durationString.Contains("s"))
                {
                    isSelected = int.TryParse(flteredInp, out recordingDuration);
                    recordingDuration = recordingDuration * 1000;
                    Console.WriteLine($"{recordingDuration} milliseconds");
                }
                else if (isSelected)
                {
                    Console.WriteLine($"{recordingDuration} milliseconds");
                }
            }
            return recordingDuration;
        }

        private static int SelectBaudRate()
        {
            int baudRate = 0;
            bool isSelected = false;

            while (!isSelected)
            {
                Console.WriteLine("\nEnter baud rate, 9 for 9600, 11 for 115200, and actual baud rate for any other");
                string baudRateString = Console.ReadLine();
                isSelected = int.TryParse(baudRateString, out baudRate);
                if (baudRate == 9 || baudRate == 9600) { baudRate = 9600; Console.WriteLine($"Selected {baudRate} baud rate"); }
                else if (baudRate == 11 || baudRate == 115200) { baudRate = 115200; Console.WriteLine($"Selected {baudRate} baud rate"); }
                else Console.WriteLine($"lol u selected {baudRate} ok. Should've selected 9600 or 115200.");
            }

            return baudRate;
        }
         
        private static int SelectPortNumber(string[] arrPortNames)
        {
            string[] ArrPortNames = SerialPort.GetPortNames();
            List<string> portNames = ArrPortNames.ToList();

            List<int> portNumbers = new List<int>();
            string ret = "Available ports: ";
            foreach (string portName in portNames)
            {
                ret += portName;
                ret += " ";
            }
            Console.WriteLine(ret);

            int selectedPortNum = 0;
            bool isSelected = false;
            while (!isSelected)
            {
                Console.WriteLine("Please enter port number from the above: (e.g. 2 or 3, not COM2 or COM3)");
                string selectedPort = Console.ReadLine();
                int.TryParse(selectedPort, out selectedPortNum);
                foreach (string portName in portNames)
                {
                    if (Convert.ToInt32(Regex.Match(portName, @"[0-9]+").Value) == selectedPortNum)
                    {
                        isSelected = true;
                        break;
                    }
                }
            }
            return selectedPortNum;
        }

        private static void WriteToFile()
        {
            Console.WriteLine("\nDone.");
            string ret = string.Empty;
            foreach (string line in rawData)
            {
                ret += line + "\n";
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\";
            DateTime now = DateTime.Now;
            string fileName = string.Empty;
            fileName += path;
            fileName += now.Year + "_";
            fileName += now.Month + "_";
            fileName += now.Day + "_";
            fileName += now.Hour + "_";
            fileName += now.Minute + "_";
            fileName += now.Second + "_";
            fileName += "recorded_data.csv";

            if (rawData.Count > 0)
            {
                File.AppendAllText(fileName, ret);
                Console.WriteLine("File saved" + " as " + fileName);
            }
            else Console.WriteLine("No data on serial port");
        }

        private static void ReadSerial()
        {
            while (port.IsOpen)
            {
                string rawLine = port.ReadExisting();
                if (rawLine.Contains("\r")) rawLine = rawLine.Replace("\r", String.Empty);
                string[] lines = rawLine.Split("\n");

                foreach (string line in lines)
                {
                    if (line.Contains("\n")) line.Replace("\n", string.Empty);
                    rawData.Add(line);
                    Console.WriteLine(line);
                }
            }
        }

        private static void PrintIntro()
        {
            Console.WriteLine("Serial COM port logger, written by Usman in .NET Core 3.1");
            Console.WriteLine("Print serial values in comma seperated format only.");
            Console.WriteLine("Other formats are not supported yet and will yield random results");
            Console.WriteLine("Report back bugs and issues, and I'll try and fix them");
            Console.WriteLine("Press ctrl + C any time to stop the app\n\n");
        }
    }
}