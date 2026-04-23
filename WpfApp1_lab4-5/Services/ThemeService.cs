using System;
using System.Linq;
using System.Windows;

namespace WpfApp1_lab4_5.Services
{
    public static class ThemeService
    {
        public static void ApplyTheme(string themeName)
        {
            string path = themeName switch
            {
                "Dark" => "/Resources/Theme/ThemeDark.xaml",
                _ => "/Resources/Theme/ThemeLight.xaml"
            };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var oldTheme = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.Contains("/Resources/Theme/"));

            var newTheme = new ResourceDictionary
            {
                Source = new Uri(path, UriKind.Relative)
            };

            if (oldTheme != null)
            {
                int index = dictionaries.IndexOf(oldTheme);
                dictionaries.Remove(oldTheme);
                dictionaries.Insert(index, newTheme);
            }
            else
            {
                dictionaries.Add(newTheme);
            }
        }
    }
}