using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace SolWPF
{
    public class PlayingCard : ToggleButton
    {
        static PlayingCard()
        {
            // Override style
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayingCard),
                new FrameworkPropertyMetadata(typeof(PlayingCard)));

            // Register Face dependency property
            FaceProperty = DependencyProperty.Register("Face",
                typeof(string), typeof(PlayingCard));
        }

        public PlayingCard(string face)
        {
            Face = face;
        }

        public string Face
        {
            get { return (string)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }

        public static DependencyProperty FaceProperty;
    }

}
