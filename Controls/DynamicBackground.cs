using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RayBin.FishImageLabel
{
        public class DynamicBackground : System.Windows.Controls.Control
        {
            static DynamicBackground()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicBackground), new FrameworkPropertyMetadata(typeof(DynamicBackground)));
            }

            public string Source
            {
                get { return (string)GetValue(SourceProperty); }
                set { SetValue(SourceProperty, value); }
            }

            public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(DynamicBackground), new FrameworkPropertyMetadata("", (a, b) =>
            {
                DynamicBackground dbgPlayer = a as DynamicBackground;
                if (dbgPlayer.Player != null)
                {
                    dbgPlayer.Player.Source = new Uri(b.NewValue.ToString().Replace("~", AppDomain.CurrentDomain.BaseDirectory), UriKind.RelativeOrAbsolute);
                }
            }));

            private MediaElement _player;

            public MediaElement Player
            {
                get { return _player; }
                set { _player = value; }
            }

            public override void OnApplyTemplate()
            {
                Player = GetTemplateChild("Media") as MediaElement;
                if (null == Player) { return; }
                Player.LoadedBehavior = MediaState.Manual;
                Player.MediaEnded += Player_MediaEnded;
                if (!string.IsNullOrEmpty(Source) && File.Exists(Source))
                {
                    Player.Source = new Uri(Source.Replace("~", AppDomain.CurrentDomain.BaseDirectory), UriKind.RelativeOrAbsolute);
                    Player.Play();
                }
                base.OnApplyTemplate();
            }

            private void Player_MediaEnded(object sender, RoutedEventArgs e)
            {
                Player.Position = TimeSpan.FromMilliseconds(1);
                Player.Play();
            }
        }
    }