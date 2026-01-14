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
        private InformationType _infoType = InformationType.Info;

        public InformationType InfoType { get { return _infoType; } // Make this part more clearer
                                          private set { 
                                                switch(value)
                                                {
                                                    case InformationType.Info:
                                                        info_path.Visibility = Visibility.Visible;
                                                        warning_path.Visibility = Visibility.Collapsed;
                                                        error_path.Visibility = Visibility.Collapsed;
                                                        break;
                                                    case InformationType.Warning:
                                                        info_path.Visibility = Visibility.Collapsed;
                                                        warning_path.Visibility = Visibility.Visible;
                                                        error_path.Visibility = Visibility.Collapsed;
                                                        break;
                                                    case InformationType.Error:
                                                        info_path.Visibility = Visibility.Collapsed;
                                                        warning_path.Visibility = Visibility.Collapsed;
                                                        error_path.Visibility = Visibility.Visible;
                                                        break;
                                                }
                                                _infoType = value;
                                               } 
                                          }
        public static void Show(string message, InformationType informationType = 0)
        {
            App.Current.Dispatcher.Invoke(() => {
                var msgBox = new UIMessageBox(message,informationType);
                msgBox.Show();
            },System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            
        }
        public string Message { get; set; } = string.Empty;

        public UIMessageBox(string message, InformationType informationType)
        {
            InitializeComponent();
            DataContext = this;
            this.Message = message;
            this.InfoType = informationType;
        }
        public UIMessageBox()
        {
            InitializeComponent();
            DataContext = this;
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

        public enum InformationType
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }
    }
}
