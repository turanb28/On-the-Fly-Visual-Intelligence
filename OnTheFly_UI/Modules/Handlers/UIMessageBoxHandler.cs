using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.Handlers
{
    public static class UIMessageBoxHandler
    {

        public static void Show(string Message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var a = (MainWindow)App.Current.MainWindow;
                a.UIMessage.Text = Message;
            });
        }
    }
}
