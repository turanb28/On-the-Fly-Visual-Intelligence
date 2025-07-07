using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public class ResultTableItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
      
        private string name = "undefined";
        public  string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        private int count = 0;
        public  int Count
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged();
            }
        }


        public ResultTableItem()
        {
        }

        public ResultTableItem(string name, int count)
        {
            Name = name;
            Count = count;
        }
        public override string ToString()
        {
            return $"{Name}: {Count}";
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
