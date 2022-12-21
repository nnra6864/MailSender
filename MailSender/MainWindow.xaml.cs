using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MailSender
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        bool _isSending = false;
        Dictionary<string, string> _senders = new Dictionary<string, string>();
        BindingList<string> _senderNames = new BindingList<string>();
        BindingList<string> _receivers = new BindingList<string>();
        HashSet<string> _blackList = new HashSet<string>
        {
            "enenragd@gmail.com",
        };

        #region UI
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            lbFrom.ItemsSource = _senderNames;
            lbTo.ItemsSource = _receivers;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_isSending) { MessageBox.Show("Please wait for the current operation to finish.", "Already Sending", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK); return; }
            await SendInIntervals();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Successful = 0;
            Failed = 0;
        }
        private void btnLoadBody_Click(object sender, RoutedEventArgs e)
        {
            LoadBody();
        }

        void AddReceiver(string receiver)
        {
            if (_blackList.Contains(receiver)) return;
            _receivers.Add(receiver);
        }

        private void btnFrom_Click(object sender, RoutedEventArgs e)
        {
            AddSenders();
        }

        private void btnTo_Click(object sender, RoutedEventArgs e)
        {
            AddReceivers();
        }
        #endregion

        #region Data
        public event PropertyChangedEventHandler ?PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string ?propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        int _repeat = 1;
        public int Repeat
        {
            get => _repeat;
            set
            {
                if (int.TryParse(value.ToString(), out _))
                {
                    if (_repeat != value)
                    {
                        _repeat = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        int _interval = 30;
        public int Interval
        {
            get => _interval;
            set
            {
                if (float.TryParse(value.ToString(), out _))
                {
                    if (_interval != value)
                    {
                        _interval = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        int _pause = 90;
        public int Pause
        {
            get => _pause;
            set
            {
                if (float.TryParse(value.ToString(), out _))
                {
                    if (_pause != value)
                    {
                        _pause = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        bool _useInterval = false;
        public bool UseInterval
        {
            get => _useInterval;
            set
            {
                _useInterval = value;
                OnPropertyChanged();
            }
        }

        bool _isHTML = false;
        public bool IsHTML
        {
            get => _isHTML;
            set
            {
                _isHTML = value;
                OnPropertyChanged();
            }
        }

        string _body = "Body";
        public string Body
        {
            get => _body;
            set
            {
                _body = value;
                OnPropertyChanged();
            }
        }

        string _subject = "Subject";
        public string Subject
        {
            get => _subject;
            set
            {
                _subject = value;
                OnPropertyChanged();
            }
        }

        int _successful = 0;
        int Successful
        {
            get => _successful;
            set
            {
                _successful = value;
                Sent = $"Sent: {Successful}";
                OnPropertyChanged();
            }
        }

        int _failed = 0;
        int Failed
        {
            get => _failed;
            set
            {
                _failed = value;
                NotSent = $"Failed: {Failed}";
                OnPropertyChanged();
            }
        }

        public string Sent
        {
            get => $"Sent: {Successful}";
            set { OnPropertyChanged(); }
        }
        public string NotSent
        {
            get => $"Failed: {Failed}";
            set { OnPropertyChanged(); }
        }
        #endregion

        #region Info
        void AddSenders()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text|*.txt";
            if (ofd.ShowDialog() == true)
            {
                _senders.Clear();
                _senderNames.Clear();
                string[] senders = System.IO.File.ReadAllLines(ofd.FileName);
                foreach (var sender in senders)
                {
                    AddSender(sender);
                }
            }
        }

        void AddSender(string sender)
        {
            var result = ReadSenderLine(sender);
            if (_blackList.Contains(result.Key)) return;
            if (_senders.TryAdd(result.Key, result.Value))
                _senderNames.Add(result.Key);
        }

        KeyValuePair<string, string> ReadSenderLine(string input)
        {
            string name = "", password = "";
            int reading = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ') { reading++; continue; }
                if (reading == 0) { name += input[i]; continue; }
                password += input[i];
            }
            return new KeyValuePair<string, string>(name, password);
        }

        void AddReceivers()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text|*.txt";
            if (ofd.ShowDialog() == true)
            {
                _receivers.Clear();
                string[] receivers = System.IO.File.ReadAllLines(ofd.FileName);
                foreach (var receiver in receivers)
                {
                    AddReceiver(receiver);
                }
            }
        }

        void LoadBody()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text|*.txt";
            if (ofd.ShowDialog() == true)
            {
                Body = string.Empty;
                string[] bodyLines = System.IO.File.ReadAllLines(ofd.FileName);
                var result = "";
                foreach (var line in bodyLines)
                {
                    result += line + Environment.NewLine;
                }
                Body = result;
            }
        }
        #endregion

        #region Sending
        async Task SendInIntervals()
        {
            _isSending = true;
            await Task.Run(async() =>
            {
                if (!UseInterval) await Send();
                while (UseInterval)
                {
                    await Send();
                    await Task.Delay((int)(Interval * 1000f));
                }
            });
            _isSending = false;
        }

        async Task Send()
        {
            for (int i = 0; i < Repeat; i++)
            {
                await SendMails();
            }
        }

        async Task SendMails()
        {
            foreach (var sender in _senders)
            {
                try
                {
                    SmtpClient client = GenerateClient(sender.Key, sender.Value);
                    foreach (var receiver in _receivers)
                    {
                        try
                        {
                            MailMessage message = GenerateMessage(sender.Key, receiver);
                            await client.SendMailAsync(message);
                            Successful = _successful + 1;
                        }
                        catch
                        {
                            Failed = _failed + 1;
                            await Task.Delay(Pause * 1000);
                        }
                    }
                }
                catch
                {
                    Failed = _failed + 1;
                    await Task.Delay(Pause * 1000);
                }
            }
        }

        MailMessage GenerateMessage(string from, string to)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(from);
            message.Subject = Subject;
            message.To.Add(new MailAddress(to));
            message.Body = Body;
            message.IsBodyHtml = IsHTML;
            return message;
        }

        SmtpClient GenerateClient(string sender, string password)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(sender, password),
                EnableSsl = true,
            };
            return smtpClient;
        }
        #endregion
    }
}