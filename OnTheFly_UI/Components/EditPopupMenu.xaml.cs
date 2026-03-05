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
    /// Interaction logic for EditPopupMenu.xaml
    /// </summary>
    public partial class EditPopupMenu : Window
    {
        public EditPopupMenu()
        {
            InitializeComponent();
        }

        public static async Task<string> Show(Point point,double a, double b)
        {
            var box = new EditPopupMenu();
            box.Left = point.X + a;
            box.Top = point.Y + b;
            bool? result = false;
            await App.Current.Dispatcher.InvokeAsync(() => {
                result = box.ShowDialog();
                
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);


            return "box.Entry";
        }
    }
}
