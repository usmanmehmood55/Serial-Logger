using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Serial_Logger.Models
{
    class Serial
    {
        public static SerialPort port = new SerialPort();
        public static List<string> rawData = new List<string>();
        public static string Seperator = ",";

        public static bool Read(string portname, int baudrate, int recordingDuration, string timeStamp, string _seperator)
        {
            if (port.IsOpen) port.Close();
            if (!string.IsNullOrEmpty(_seperator)) Seperator = _seperator;

            port.ReadTimeout = 100;
            port.PortName = portname;
            port.BaudRate = baudrate;
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Open();

            Task t = Task.Run(() => { ReadSerial(); });

            Thread.Sleep(recordingDuration);
            port.Close();
            t.Dispose();

            WriteToFile();
            return true;
        }

        public static void ReadSerial()
        {
            while (port.IsOpen)
            {
                char[] dataLine = new char[4096];
                int arrayLen = dataLine.Length;
                int nBytes = port.Read(dataLine, 0, arrayLen);
                char[] reducedLines = new char[nBytes];
                Array.Copy(dataLine, reducedLines, nBytes);

                string rawLine = new string(reducedLines);

                if (rawLine.Contains("\n")) rawLine = rawLine.Replace("\n", string.Empty);
                string[] lines = rawLine.Split('\r');

                foreach (string line in lines)
                {
                    string thisLine = line;
                    // if (thisLine.Contains("\n")) thisLine = thisLine.Replace("\n", string.Empty);
                    if (thisLine.Contains(Seperator)) thisLine = thisLine.Replace(Seperator, ",");
                    if (!string.IsNullOrEmpty(thisLine)) rawData.Add(thisLine);
                    Console.WriteLine(thisLine);
                }
            }
        }

        public static void WriteToFile()
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
    }
}
