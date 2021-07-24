using AudioMapper.Models;
using MaterialDesignThemes.Wpf;

namespace AudioMapper.Helpers
{
    internal class ThemeHelper
    {
        public static System.Windows.Media.Color DefaultBackgroundColor
        {
            get
            {
                if (SettingsHelper.Settings.Theme_IsDarkTheme)
                {
                    return Theme.Dark.MaterialDesignToolBackground;
                }
                else
                {
                    return Theme.Light.MaterialDesignToolBackground;
                }
            }
        }

        public static System.Windows.Media.Color DefaultForegroundColor
        {
            get
            {
                if (SettingsHelper.Settings.Theme_IsDarkTheme)
                {
                    return Theme.Dark.MaterialDesignBody;
                }
                else
                {
                    return Theme.Light.MaterialDesignBody;
                }
            }
        }

        public static void BindTheme() => BindTheme(SettingsHelper.Settings);

        public static void BindTheme(UserSettings settings)
        {
            if (settings != null)
            {
                settings.Theme_IsDarkTheme_Changed += ChangeTheme;
                settings.Theme_PrimaryColor_Changed += ChangePrimaryColor;
                settings.Theme_SecondaryColor_Changed += ChangeSecondaryColor;
            }
        }

        public static void ChangePrimaryColor(object sender, System.Windows.Media.Color color)
        {
            //OG MD
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetPrimaryColor(color);
            palette.SetTheme(theme);

            //Ext MD
            var paletteExt = new PaletteHelper();
            var themeExt = paletteExt.GetTheme();
            themeExt.SetPrimaryColor(color);
            paletteExt.SetTheme(themeExt);
        }

        public static void ChangeSecondaryColor(object sender, System.Windows.Media.Color color)
        {
            //OG MD
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetSecondaryColor(color);
            palette.SetTheme(theme);

            //Ext MD
            var paletteExt = new PaletteHelper();
            var themeExt = paletteExt.GetTheme();
            themeExt.SetSecondaryColor(color);
            paletteExt.SetTheme(themeExt);
        }

        public static void ChangeTheme(object sender, bool isDarkTheme)
        {
            //OG MD
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();

            if (isDarkTheme)
            {
                theme.SetBaseTheme((IBaseTheme)new MaterialDesignDarkTheme());
            }
            else
            {
                theme.SetBaseTheme((IBaseTheme)new MaterialDesignLightTheme());
            }

            palette.SetTheme(theme);

            //Ext MD
            var paletteExt = new PaletteHelper();
            var themeExt = palette.GetTheme();

            if (isDarkTheme)
            {
                themeExt.SetBaseTheme((IBaseTheme)new MaterialDesignDarkTheme());
            }
            else
            {
                themeExt.SetBaseTheme((IBaseTheme)new MaterialDesignLightTheme());
            }

            paletteExt.SetTheme(themeExt);
        }

        public static void RefreshTheme()
        {
            if (SettingsHelper.Settings != null)
            {
                ChangeTheme(null, SettingsHelper.Settings.Theme_IsDarkTheme);
                ChangePrimaryColor(null, SettingsHelper.Settings.Theme_PrimaryColor);
                ChangeSecondaryColor(null, SettingsHelper.Settings.Theme_SecondaryColor);
            }
        }

        public static void UnBindTheme() => UnBindTheme(SettingsHelper.Settings);

        public static void UnBindTheme(UserSettings settings)
        {
            if (settings != null)
            {
                settings.Theme_IsDarkTheme_Changed -= ChangeTheme;
                settings.Theme_PrimaryColor_Changed -= ChangePrimaryColor;
                settings.Theme_SecondaryColor_Changed -= ChangeSecondaryColor;
            }
        }
    }
}