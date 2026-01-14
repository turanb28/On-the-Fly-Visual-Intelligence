using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnTheFly_UI.Components
{
    /// <summary>
    /// Interaction logic for UIUserEntry.xaml
    /// </summary>
    public partial class UIUserEntry : Window
    {


        public string Message { get; set; } = string.Empty;
        public string Entry { get; set; } = string.Empty;

        public UIUserEntry()
        {
            InitializeComponent();
            DataContext = this;
            entry.Text = "https://www.pexels.com/download/video/35217602/";
        }

        public UIUserEntry(string message)
        {
            InitializeComponent();
            DataContext = this;
            this.Message = message;
        }

        public static async Task<string> Show(string message)
        {
            var box = new UIUserEntry(message);
            bool? result = false;
            await App.Current.Dispatcher.InvokeAsync(() => {
                result = box.ShowDialog();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);


            return box.Entry;
        }

        

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Entry = entry.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
