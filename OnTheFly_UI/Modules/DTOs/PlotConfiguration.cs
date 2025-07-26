using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OnTheFly_UI.Modules.DTOs
{
    public sealed class PlotConfiguration
    {
        public List<System.Drawing.Color> ObjectColors { get; set; } = 
            new List<System.Drawing.Color>()
        {
            System.Drawing.Color.Purple,         // Pure purple
            System.Drawing.Color.Green,          // Pure green
            System.Drawing.Color.Blue,           // Pure blue
            System.Drawing.Color.Yellow,         // Pure yellow
            System.Drawing.Color.Cyan,           // Pure cyan
            System.Drawing.Color.Magenta,        // Pure magenta
            System.Drawing.Color.Orange,         // Strong warm orange
            System.Drawing.Color.Red,            // Deep red
            System.Drawing.Color.Lime,           // Bright lime green
            System.Drawing.Color.DeepSkyBlue,    // Vivid sky blue
            System.Drawing.Color.Gold,           // Rich gold
            System.Drawing.Color.Crimson,        // Deep red
            System.Drawing.Color.Turquoise,      // Distinctive blue-green
            System.Drawing.Color.Orchid,         // Unique pinkish-purple
            System.Drawing.Color.Chartreuse,     // Yellow-green
            System.Drawing.Color.RoyalBlue,      // Deep strong blue
            System.Drawing.Color.Sienna,         // Reddish-brown
            System.Drawing.Color.SpringGreen,    // Bright green with a hint of blue
            System.Drawing.Color.Coral,          // Pinkish-orange
            System.Drawing.Color.OliveDrab,      // Earthy green
            System.Drawing.Color.SteelBlue,      // Muted cool blue
            System.Drawing.Color.HotPink,        // Intense pink
            System.Drawing.Color.DarkOrange,     // Deep saturated orange
            System.Drawing.Color.MediumPurple,   // Well-balanced purple
            System.Drawing.Color.LightSeaGreen,  // Distinctive mix of green and blue
            System.Drawing.Color.Firebrick,      // Dark reddish tone
            System.Drawing.Color.DodgerBlue,     // Very vibrant blue
            System.Drawing.Color.LawnGreen,      // Almost neon green
            System.Drawing.Color.Salmon,         // Soft pinkish-red
            System.Drawing.Color.MediumSlateBlue // Unique bluish-purple
        };
        public int FontSize { get; set; } = 12;
        public int BorderThickness { get; set; } = 2;
        public System.Drawing.Color FontColor { get; set; } = System.Drawing.Color.White;
        public System.Drawing.FontFamily FontFamily { get; set; } = System.Drawing.FontFamily.GenericSansSerif;
        public System.Drawing.Font Font { get => new System.Drawing.Font(FontFamily, FontSize);  }
    }
}
