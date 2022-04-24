using ShadowTH_Text_Editor.Helpers;
using System.Windows;

namespace ShadowTH_Text_Editor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // TODO: Saved value restoration?
            // ThemeHelper.ApplySkin(Skin.Dark);
        }
    }
}
