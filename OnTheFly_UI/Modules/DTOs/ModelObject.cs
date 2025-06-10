using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public class ModelObject : INotifyPropertyChanged
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public override bool Equals(Object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return (this.Path == ((ModelObject)obj).Path);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
