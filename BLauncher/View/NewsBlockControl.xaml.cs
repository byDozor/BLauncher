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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLauncher.View
{
    /// <summary>
    /// Interaction logic for NewsBlockControl.xaml
    /// </summary>
    public partial class NewsBlockControl : UserControl
    {
        public NewsBlockControl()
        {
            InitializeComponent();
        }
        // Властивість залежності для фонової картинки
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(NewsBlockControl));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Властивість залежності для тексту новини
        public static readonly DependencyProperty NewsTextProperty =
            DependencyProperty.Register("NewsText", typeof(string), typeof(NewsBlockControl));

        public string NewsText
        {
            get { return (string)GetValue(NewsTextProperty); }
            set { SetValue(NewsTextProperty, value); }
        }
    }
}
