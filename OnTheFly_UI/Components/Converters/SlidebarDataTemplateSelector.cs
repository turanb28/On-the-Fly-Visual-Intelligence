using OnTheFly_UI.Modules.DTOs;
using OnTheFly_UI.Modules.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OnTheFly_UI.Components.Converters
{
    public class SlidebarDataTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate ImageTemplate { get; set; }
        public required DataTemplate VideoTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var request = item as RequestObject;

            return request.SourceType == RequestSourceType.Image ? ImageTemplate : VideoTemplate;
        }   
    }
}
