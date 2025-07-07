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
                if(Message == null)
                    return;
                var a = (MainWindow)App.Current.MainWindow;

                if (a.UIMessage == null)
                    return;
                a.UIMessage.Text = Message;
            });
        }
    }
}
