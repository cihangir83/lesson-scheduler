using System;
using System.Linq;
using System.Windows;

namespace LessonScheduler.Services
{
    // Dark desteği kaldırıldı; tek seçenek Light
    public enum ThemeType
    {
        Light
    }

    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        public event EventHandler<ThemeType>? ThemeChanged;

        private ThemeType _currentTheme = ThemeType.Light;

        public ThemeType CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        private ThemeService() { }

        public void SetTheme(ThemeType theme)
        {
            // Sadece Light desteklenir
            CurrentTheme = ThemeType.Light;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var app = Application.Current;
            if (app == null) return;

            var existingTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme.xaml") == true);

            if (existingTheme != null)
            {
                app.Resources.MergedDictionaries.Remove(existingTheme);
            }

            var themeUri = new Uri("pack://application:,,,/Themes/LightTheme.xaml");
            var newTheme = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Add(newTheme);
        }

        // Artık tema değiştirme yok; no-op bırakıldı (geri çağrılar kırılmasın)
        public void ToggleTheme()
        {
            // Intentionally no-op since only Light is supported
        }

        public string GetThemeDisplayName(ThemeType theme)
        {
            return "🌞 Açık Tema";
        }

        public string GetCurrentThemeDisplayName()
        {
            return GetThemeDisplayName(CurrentTheme);
        }
    }
}