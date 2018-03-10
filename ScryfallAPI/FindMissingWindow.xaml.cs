namespace ScryfallAPI
{
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    public partial class FindMissingWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string _mainText;
        public string MainText {
            get { return _mainText; }
            set {
                if (value != _mainText)
                {
                    _mainText = value;
                    NotifyPropertyChanged("MainText");
                }
            }
        }

        public ICommand GetCollectionCardsCommand { get; set; }
        public ICommand GetRequestedCardsCommand { get; set; }
        public ICommand CalculateDiffCommand { get; set; }
        public ICommand SaveFileCommand { get; set; }
        
        public FindMissingWindow()
        {
            InitializeComponent();

            DataContext = this;

            GetCollectionCardsCommand = new SimpleCommand(GetCards);
            GetRequestedCardsCommand = new SimpleCommand(GetCards);
            CalculateDiffCommand = new SimpleCommand(GetCards);
            SaveFileCommand = new SimpleCommand(SaveFile);

            MainText = "adsdas";
        }

        private string PrintRarity(string rarity) {
            switch (rarity)
            {
                case "common": return "[C] ";
                case "uncommon" : return "[U] ";
                case "rare": return "[R] ";
                case "mythic": return "[M] ";
                default: return $"[???{rarity}] ";
            }
        }

        private void SaveFile()
        {
            var save = new SaveFileDialog();
            save.FileName = "missing";
            save.DefaultExt = ".txt";
            if (save.ShowDialog(this) == true)
            {
                File.WriteAllText(save.FileName, MainText);
            }
        }

        private void GetCards()
        {
            try
            {
                var d = new OpenFileDialog();
                if (d.ShowDialog() == true)
                {
                    MainText += $"Opening File {d.FileName}";

                }
            }
            catch (Exception e)
            {
                MainText = e.ToString();
            }
        }

        private static string GetResponceFrom(string url)
        {
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var resStream = response.GetResponseStream();
            var reader = new StreamReader(resStream);
            var appiResponse = reader.ReadToEnd();
            return appiResponse;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
