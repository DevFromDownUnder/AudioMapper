using PropertyChanged;
using System;
using System.Windows.Media;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class UserSettings
    {
        #region Change Tracking

        public bool IsChanged { get; set; }

        #endregion Change Tracking

        #region Theme Event Handler Workarounds

        public event EventHandler<bool> Theme_IsDarkTheme_Changed;

        public event EventHandler<Color> Theme_PrimaryColor_Changed;

        public event EventHandler<Color> Theme_SecondaryColor_Changed;

        public void OnTheme_IsDarkThemeChanged() => Theme_IsDarkTheme_Changed?.Invoke(this, Theme_IsDarkTheme);

        public void OnTheme_PrimaryColorChanged() => Theme_PrimaryColor_Changed?.Invoke(this, Theme_PrimaryColor);

        public void OnTheme_SecondaryColorChanged() => Theme_SecondaryColor_Changed?.Invoke(this, Theme_SecondaryColor);

        #endregion Theme Event Handler Workarounds

        #region Theme Settings

        public bool Theme_IsDarkTheme { get; set; }
        public Color Theme_PrimaryColor { get; set; }
        public Color Theme_SecondaryColor { get; set; }

        #endregion Theme Settings
    }
}