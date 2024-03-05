using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace RayBin.FishImageLabel
{
    public class IconButton : Button
    {
        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }        
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(IconButton));

        public SolidColorBrush MouseOverColor
        {
            get { return (SolidColorBrush)GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(SolidColorBrush), typeof(IconButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public SolidColorBrush MouseOverBackColor
        {
            get { return (SolidColorBrush)GetValue(MouseOverBackColorProperty); }
            set { SetValue(MouseOverBackColorProperty, value); }
        }
        public static readonly DependencyProperty MouseOverBackColorProperty = DependencyProperty.Register("MouseOverBackColor", typeof(SolidColorBrush), typeof(IconButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public double ButtonHeight
        {
            get { return (double)GetValue(ButtonHeightProperty); }
            set { SetValue(ButtonHeightProperty, value);}
        }
        public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register("ButtonHeight", typeof(double), typeof(IconButton), new PropertyMetadata(32.0));

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value);}
        }
        public static readonly DependencyProperty IconTextProperty = DependencyProperty.Register("IconText", typeof(string), typeof(IconButton), new PropertyMetadata(""));

        public double IconTextSize
        {
            get { return (double)GetValue(IconTextSizeProperty); }
            set { SetValue(IconTextSizeProperty, value);}
        }
        public static readonly DependencyProperty IconTextSizeProperty = DependencyProperty.Register("IconTextSize", typeof(double), typeof(IconButton), new PropertyMetadata(20.0));

        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value);}
        }
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(IconButton), new PropertyMetadata(16.0));

        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value);}
        }
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(IconButton), new PropertyMetadata(16.0));
    }
}