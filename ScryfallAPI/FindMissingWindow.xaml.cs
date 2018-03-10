namespace ScryfallAPI
{
    using CsvHelper;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
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
        public SimpleCommand CalculateDiffCommand { get; set; }
        public ICommand SaveFileCommand { get; set; }

        public bool CollectionLoaded { get; set; }
        public bool RequestSetLoaded { get; set; }

        private Dictionary<string, int> Collection;
        private Dictionary<string, int> Request;

        public FindMissingWindow()
        {
            InitializeComponent();

            DataContext = this;

            GetCollectionCardsCommand = new SimpleCommand(GetCollection);
            GetRequestedCardsCommand = new SimpleCommand(GetRequest);
            CalculateDiffCommand = new SimpleCommand(GetDiff, () => CollectionLoaded && RequestSetLoaded);
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

        private void GetCollection()
        {
            try
            {
                var d = new OpenFileDialog();
                if (d.ShowDialog() == true)
                {
                    MainText += $"Opening File {d.FileName}";

                    using (var stream = File.OpenRead(d.FileName))
                    using (var reader = new StreamReader(stream))
                    using (var csvReader = new CsvReader(reader))
                    {
                        csvReader.Read();
                        csvReader.ReadHeader();
                        Collection = new Dictionary<string, int>();
                        while (csvReader.Read())
                        {
                            var cnt = csvReader.GetField<int>("Count");
                            var name = csvReader.GetField<string>("Name");

                            if (!Collection.ContainsKey(name))
                            {
                                Collection[name] = 0;
                            }

                            Collection[name] += cnt;
                        }

                        CollectionLoaded = true;
                        CalculateDiffCommand.UpdateCanExecute();

                        MainText += $"{Environment.NewLine}{Collection.Count} different cards found";
                        //foreach (var e in Collection)
                        //{
                        //    MainText += $"{Environment.NewLine}{e.Value} {e.Key}";
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                MainText = e.ToString();
            }
        }

        private void GetRequest()
        {
            try
            {
                var d = new OpenFileDialog();
                if (d.ShowDialog() == true)
                {
                    MainText += $"Opening File {d.FileName}";

                    using (var stream = File.OpenRead(d.FileName))
                    using (var reader = new StreamReader(stream))
                    {
                        Request = new Dictionary<string, int>();

                        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                        {
                            var cnt = 1;
                            var name = Regex.Replace(line,"\\[.\\] ","");

                            if (!Request.ContainsKey(name))
                            {
                                Request[name] = 0;
                            }

                            Request[name] += cnt;
                        }
                        RequestSetLoaded = true;
                        CalculateDiffCommand.UpdateCanExecute();

                        MainText += $"{Environment.NewLine}{Request.Count} different cards found";
                        foreach (var e in Request)
                        {
                            MainText += $"{Environment.NewLine}{e.Value} {e.Key}";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MainText = e.ToString();
            }
        }

        private void GetDiff()
        {
            try
            {
                MainText = "Missing cards:";

                var missing = new Dictionary<string, int>();
                foreach (var r in Request)
                {
                    var reqName = r.Key;
                    var reqCnt = r.Value;

                    if (!Collection.ContainsKey(reqName))
                    {
                        missing[reqName] = reqCnt;
                    }
                    else if (Collection[reqName] < reqCnt)
                    {
                        missing[reqName] = reqCnt - Collection[reqName];
                    }
                }
                var sortedKeys = missing.Keys.ToList();
                sortedKeys.Sort();
                foreach (var key in sortedKeys)
                {
                    MainText += $"{Environment.NewLine}{missing[key]} {key}";
                }
            }
            catch (Exception e)
            {
                MainText = e.ToString();
            }
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
