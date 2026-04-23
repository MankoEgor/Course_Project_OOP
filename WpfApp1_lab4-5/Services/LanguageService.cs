using System;
using System.Linq;
using System.Windows;

namespace WpfApp1_lab4_5.Services
{
    public static class LanguageService
    {
        public static void ChangeLanguage(string languageCode)
        {
            string path = languageCode switch
            {
                "en" => "Resources/Languages/Lang.en.xaml",
                _ => "Resources/Languages/Lang.ru.xaml"
            };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var oldDictionary = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.Contains("Resources/Languages/Strings."));

            if (oldDictionary != null)
                dictionaries.Remove(oldDictionary);

            dictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(path, UriKind.Relative)
            });
        }
    }
}