using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace RayBin.FishImageLabel
{
    public class NotifyIconMenu : MenuItem
    {
        public SolidColorBrush IconColor
        {
            get { return (SolidColorBrush)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }
        public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register("IconColor", typeof(SolidColorBrush), typeof(NotifyIconMenu), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(NotifyIconMenu), new PropertyMetadata(16.0));

        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(NotifyIconMenu), new PropertyMetadata(16.0));

        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(NotifyIconMenu));

    }
}