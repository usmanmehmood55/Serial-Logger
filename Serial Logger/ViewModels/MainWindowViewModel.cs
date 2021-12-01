using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.IO.Ports;
using Serial_Logger.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Serial_Logger.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _selectedComPort;
        private int _selectedBaudRate;
        private string _selectedTimeStamp;
        private string _selectedTimeUnit;
        private string _title = "Serial Logger";
        private string _seperator;
        private int _logTime;
        private string _buttonText = "Connect and Log";

        private ObservableCollection<string> _comPortList = new ObservableCollection<string>(
            SerialPort.GetPortNames());
        private ObservableCollection<string> _units = new ObservableCollection<string>(
            new string[] { "sec", "msec", "min", "hour" });
        private ObservableCollection<string> _timeStampList = new ObservableCollection<string>(
            new string[] { "s", "ms", "h:m:s:ms", "m:s:ms" });
        private ObservableCollection<string> _BaudRates = new ObservableCollection<string>(
            new string[] { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" });

        public string SelectedComPort { get { return _selectedComPort; } set { SetProperty(ref _selectedComPort, value); } }
        public int SelectedBaudRate { get { return _selectedBaudRate; } set { SetProperty(ref _selectedBaudRate, value); } }
        public string SelectedTimeUnit { get { return _selectedTimeUnit; } set { _selectedTimeUnit = value; } }
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public string SelectedTimeStamp { get { return _selectedTimeStamp; } set { _selectedTimeStamp = value; } }
        public string Seperator { get { return _seperator; } set { _seperator = value; } }
        public int LogTime { get { return _logTime; } set { _logTime = value; } }
        public string ButtonText { get { return _buttonText; } set { SetProperty(ref _buttonText, value); } }

        public ObservableCollection<string> Units { get { return _units; } set { _units = value; } }
        public ObservableCollection<string> BaudRates { get { return _BaudRates; } set { _BaudRates = value; } }
        public ObservableCollection<string> TimeStampList { get { return _timeStampList; } set { _timeStampList = value; } }
        public ObservableCollection<string> ComPortList { get { return _comPortList; } set { _comPortList = value; } }

        private bool canLog = true;

        public MainWindowViewModel()
        {
            ConnectAndLog = new DelegateCommand(LogSerial, () => canLog);
        }

        public DelegateCommand ConnectAndLog { get; private set; }
        private void LogSerial()
        {
            ButtonText = "Processing";
            Thread.Sleep(100);

            bool validArgs =
                SelectedComPort != null &&
                SelectedBaudRate > 0 &&
                SelectedTimeStamp != null &&
                LogTime > 0 &&
                SelectedTimeUnit != null;

            switch (SelectedTimeUnit)
            {
                case "sec":
                    LogTime = LogTime * 1000;
                    break;

                case "msec":
                    // no need to change
                    break;

                case "min":
                    LogTime = LogTime * 60 * 1000;
                    break;

                case "hour":
                    LogTime = LogTime * 60 * 60 * 1000;
                    break;

                default:
                    break;
            }
            ;
            ButtonText = validArgs ? ButtonText : "Wrong Parameters";
            bool completed = false;
            if (validArgs)
            {
                ButtonText = "Processing";

                canLog = false;
                ConnectAndLog.RaiseCanExecuteChanged();



                Task readTask = Task.Run(() =>
                { completed = Serial.Read(SelectedComPort, SelectedBaudRate, LogTime, SelectedTimeStamp, Seperator); });
                TaskAwaiter awaiter = readTask.GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    ButtonText = "Done, saved on Desktop";
                    canLog = true;
                    ConnectAndLog.RaiseCanExecuteChanged();
                    readTask.Dispose();
                });
            }
        }
    }
}
