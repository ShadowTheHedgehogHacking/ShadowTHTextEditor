using ShadowTH_Text_Editor.Helpers;
using System.Windows;


namespace ShadowTH_Text_Editor
{
    /// <summary>
    /// Interaction logic for TranslationMapper.xaml
    /// </summary>
    public partial class TranslationMapper : Window
    {
        public TranslationMapper()
        {
            InitializeComponent();
            ThemeHelper.ApplySkin(Skin.Dark);
            SetGroupBoxBorder(0.1d);

        }

        private void Button_PreviewJPAudio_SEQ00_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_PreviewJPAudio_SEQ01_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_PreviewJPAudio_SEQ02_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_PreviewENAudio_SEQ00_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_PreviewENAudio_SEQ01_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_PreviewENAudio_SEQ02_Click(object sender, RoutedEventArgs e)
        {


        }

        private void Button_LoadTargetFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Previous_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetGroupBoxBorder(double multiplier)
        {
            double thickness = 1.0d * multiplier;
            var value = new Thickness(thickness, thickness, thickness, thickness);
            GroupBoxEN.BorderThickness = value;
            GroupBoxTR.BorderThickness = value;
            GroupBoxJP.BorderThickness = value;
        }
    }
}
