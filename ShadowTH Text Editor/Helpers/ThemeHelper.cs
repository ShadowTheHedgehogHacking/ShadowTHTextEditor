using System;
using System.ComponentModel;
using System.Windows;

namespace ShadowTH_Text_Editor.Helpers
{
    public enum Skin
    {
        [Description("Dark")]
        Dark,
        [Description("Light")]
        Light,
    }

    internal static class ThemeHelper
    {
        /// <summary>
        /// Applies the specified skin to the application resources.
        /// </summary>
        /// <param name="skin">The skin to apply.</param>
        public static void ApplySkin(Skin skin)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            switch (skin)
            {
                case Skin.Dark:
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Themes\\DarkTheme.xaml", UriKind.Relative) });
                    break;
                case Skin.Light:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}