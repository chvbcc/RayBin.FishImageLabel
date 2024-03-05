using System.Windows;
using System.Windows.Controls;

namespace RayBin.FishImageLabel
{
    public class ExtendTextBox : TextBox
    {
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(ExtendTextBox));
    }
}