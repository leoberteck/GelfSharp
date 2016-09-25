using GelfSharpCore.Enum;
using GelfSharpCore.src;
using GelfSharpInterface.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;

namespace GelfSharpInterface.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private string address = "http://192.168.0.106:13000";
        public string Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    RaisePropertyChanged("Address");
                }
            }
        }

        private string shortMessage;
        public string ShortMessage
        {
            get { return shortMessage; }
            set
            {
                if (shortMessage != value)
                {
                    shortMessage = value;
                    RaisePropertyChanged("ShortMessage");
                }
            }
        }

        private string longMessage;
        public string LongMessage
        {
            get { return longMessage; }
            set
            {
                if (longMessage != value)
                {
                    longMessage = value;
                    RaisePropertyChanged("LongMessage");
                }
            }
        }

        private LogLevel level;
        public LogLevel Level
        {
            get { return level; }
            set
            {
                if (level != value)
                {
                    level = value;
                    RaisePropertyChanged("Level");
                }
            }
        }

        private ObservableCollection<AditionalField> aditionalFields;
        public ObservableCollection<AditionalField> AditionalFields
        {
            get { return aditionalFields; }
            set
            {
                if (aditionalFields != value)
                {
                    aditionalFields = value;
                    RaisePropertyChanged("AditionalFields");
                }
            }
        }

        private ObservableCollection<Response> listResponse;
        public ObservableCollection<Response> ListResponse
        {
            get { return listResponse; }
            set
            {
                if (listResponse != value)
                {
                    listResponse = value;
                    RaisePropertyChanged("ListResponse");
                }
            }
        }

        public IEnumerable<LogLevel> ListLogLevels { get { return Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>(); } }

        public RelayCommand AddAditionalField { get; set; }
        public RelayCommand SendTapped { get; set; }
        public RelayCommand StressTapped { get; set; }
        private DispatcherTimer timer{ get; set; } 
        public MainViewModel()
        {
            timer = new DispatcherTimer();
            timer.Tick += (object sender, object e) => { SendMessage(); };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            Level = ListLogLevels.First();
            AditionalFields = new ObservableCollection<AditionalField>();
            ListResponse = new ObservableCollection<Response>();
            AddAditionalField = new RelayCommand(() =>
            {
                AditionalFields.Add(new AditionalField() { Remove = new RelayCommand<AditionalField>(RemoveAditionalField)});
            });
            SendTapped = new RelayCommand(SendMessage);
            StressTapped = new RelayCommand(StressTest);
        }

        private void RemoveAditionalField(AditionalField fieldToRemove)
        {
            AditionalFields.Remove(fieldToRemove);
        }

        private void StressTest()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
        }

        private async void SendMessage()
        {
            GelfShrapClient.Init(Address, "gelf");
            var message = new GelfMessage
            {
                host = NetworkInformation.GetHostNames().Where(t => t.Type == HostNameType.DomainName).First().DisplayName,
                level = (int)Level,
                short_message = ShortMessage,
                full_message = LongMessage
            };

            foreach (var one in AditionalFields)
            {
                message.Add(one.Key, one.Value);
            }
            try
            {
                var response = await GelfShrapClient.SendMessageAsync(message);
                await HandleResponse(response);
            }
            catch (Exception e)
            {
                HandleResponse(e.Message, false);
            }
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var msg = await reader.ReadToEndAsync();
            msg = msg?.Length > 0 ? msg : "Success sending data to: " + address;
            HandleResponse(msg, response.IsSuccessStatusCode);
        }

        private void HandleResponse(string _header, bool _isSuccess)
        {
            var responseViewModel = new Response { Header = DateTime.Now.ToString("HH:mm::ss.fff tt") + " - " + _header, IsSuccess = _isSuccess };
            ListResponse.Insert(0, responseViewModel);
        }

        public class Response : NotifyPropertyChangedBase
        {
            private string header;
            public string Header
            {
                get { return header; }
                set
                {
                    if (header != value)
                    {
                        header = value;
                        RaisePropertyChanged("Header");
                    }
                }
            }

            private bool isSuccess;
            public bool IsSuccess
            {
                get { return isSuccess; }
                set
                {
                    if (isSuccess != value)
                    {
                        isSuccess = value;
                        RaisePropertyChanged("IsSuccess");
                    }
                }
            }
        }

        public class AditionalField : NotifyPropertyChangedBase
        {
            private string key;
            public string Key
            {
                get { return key; }
                set
                {
                    if (key != value)
                    {
                        key = value;
                        RaisePropertyChanged("Key");
                    }
                }
            }

            private string _value;
            public string Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        RaisePropertyChanged("Value");
                    }
                }
            }

            public RelayCommand<AditionalField> Remove { get; set; }
        }
    }
}
