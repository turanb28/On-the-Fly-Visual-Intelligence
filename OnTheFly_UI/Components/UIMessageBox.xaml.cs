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
    /// Interaction logic for UIMessageBox.xaml
    /// </summary>
    public partial class UIMessageBox : Window
    {
        public static void Show(string message)
        {
            App.Current.Dispatcher.Invoke(() => {
                var msgBox = new UIMessageBox();
                msgBox.Message = message;
                msgBox.ShowDialog();
            });
            
        }
        public string Message { get; set; } = string.Empty;
        public UIMessageBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
