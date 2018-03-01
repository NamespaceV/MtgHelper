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

    public partial class MainWindow : Window, INotifyPropertyChanged
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

        private string _setSymbol;
        public string SetSymbol
        {
            get { return _setSymbol; }
            set
            {
                if (value != _setSymbol)
                {
                    _setSymbol = value;
                    NotifyPropertyChanged("Uri");
                }
            }
        }

        public ICommand GetCardsCommand { get; set; }
        public ICommand SaveFileCommand { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            GetCardsCommand = new SimpleCommand(GetCards);
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
            save.FileName = SetSymbol;
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
                string url = "https://api.scryfall.com/sets/" + SetSymbol;
                string appiResponse = GetResponceFrom(url);
                dynamic parsedJson = JsonConvert.DeserializeObject(appiResponse);
                var result = "";

                if (parsedJson["object"].Value != "set") throw new Exception("Not a list");
                var nextUri = parsedJson.search_uri.ToString();

                while (nextUri != null)
                {
                    Thread.Sleep(200);
                    appiResponse = GetResponceFrom(nextUri);
                    parsedJson = JsonConvert.DeserializeObject(appiResponse);
                    if (parsedJson["object"].Value != "list") throw new Exception("Not a list");
                    foreach (var listObj in parsedJson["data"])
                    {
                        if (listObj["object"].Value != "card") throw new Exception("Not a card");
                        result += PrintRarity(listObj["rarity"].Value);
                        var cardName = listObj["name"].Value as string;
                        result += cardName + Environment.NewLine;
                    }
                    if (parsedJson["has_more"].Value == true)
                    {
                        nextUri = parsedJson["next_page"].Value;
                    }
                    else
                    {
                        nextUri = null;
                    }
                }

                MainText = result;// JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
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
