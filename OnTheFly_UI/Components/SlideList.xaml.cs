using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OnTheFly_UI.Modules.DTOs;

namespace OnTheFly_UI.Components
{
    /// <summary>
    /// Interaction logic for SlideList.xaml
    /// </summary>
    public partial class SlideList : UserControl
    {
        private ObservableCollection<RequestObject> _values;

        public ObservableCollection<RequestObject> Values
        {
            get => _values;
            set
            {
                if (_values != null)
                    _values.CollectionChanged -= Value_CollectionChanged;
                _values = value;
                if (_values != null)
                    _values.CollectionChanged += Value_CollectionChanged;
            }
        }

        private void Value_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            listBox.Dispatcher.BeginInvoke(   /// Configurate this part
            new Action(
                () =>
                {
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
                }),
                        System.Windows.Threading.DispatcherPriority.Input
                        );
        }

        public event SelectionChangedEventHandler SelectionChanged;
        public SlideList()
        {
            InitializeComponent();
            DataContext = this;

        }


        private void StackPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var a = GetScrollViewer(listBox) as ScrollViewer;

            if (a == null)
                MessageBox.Show("this is null");


            a.ScrollToHorizontalOffset(a.HorizontalOffset - (Math.Sign(e.Delta)));


        }

        



        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            SelectionChanged?.Invoke(this,e);
            
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
    }
}
