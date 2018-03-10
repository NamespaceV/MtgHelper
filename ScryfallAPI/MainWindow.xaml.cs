namespace ScryfallAPI
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ICommand DownloadSetCommand { get; set; }
        public ICommand FindMissingCommand { get; set; }
        

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            DownloadSetCommand = new SimpleCommand(ShowDialog<DownloadSetWindow>);
            FindMissingCommand = new SimpleCommand(ShowDialog<FindMissingWindow>);
        }

        private void ShowDialog<T>()  where T:Window, new()
        {
            var win = new T
            {
                Owner = this
            };
            win.ShowDialog();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
