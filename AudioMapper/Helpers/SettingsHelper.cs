using AudioMapper.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace AudioMapper.Helpers
{
    public class SettingsHelper
    {
        public const string DEFAULT_SETTINGS_PATH = "settings.json";
        private static readonly UserSettings settings = GetDefaultSettings();

        public static UserSettings Settings { get => settings; }

        public static void ExportUserSettings(UserSettings value, string path)
        {
            File.WriteAllText(path, FormatUserSettings(value));
        }

        public static string FormatUserSettings(UserSettings value)
        {
            return JsonSerializer.Serialize(value, new JsonSerializerOptions() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });
        }

        public static UserSettings GetDefaultSettings()
        {
            return new UserSettings()
            {
                Theme_IsDarkTheme = true,
                Theme_PrimaryColor = Color.FromArgb(255, 33, 150, 243),
                Theme_SecondaryColor = Color.FromArgb(255, 1, 66, 96)
            };
        }

        public static string GetSettingsJson()
        {
            return FormatUserSettings(Settings);
        }

        public static bool LoadSettings(bool track = true, string path = DEFAULT_SETTINGS_PATH)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var settings = File.ReadAllText(path);

            UpdateSettings(ParseUserSettingsJson(settings), track);

            return true;
        }

        public static UserSettings ParseUserSettingsJson(string json)
        {
            return JsonSerializer.Deserialize<UserSettings>(json);
        }

        public static void SaveSettings(bool track = true, string path = DEFAULT_SETTINGS_PATH)
        {
            File.WriteAllText(path, GetSettingsJson());

            if (track)
            {
                Settings.IsChanged = false;
            }
        }

        public static void ShallowCopy(UserSettings source, UserSettings destination, bool track)
        {
            if (source == null || destination == null)
            {
                return;
            }

            //Hand written copy, to avoid WPF unbinding issues setting normal assingment
            destination.Theme_IsDarkTheme = source.Theme_IsDarkTheme;
            destination.Theme_PrimaryColor = source.Theme_PrimaryColor;
            destination.Theme_SecondaryColor = source.Theme_SecondaryColor;

            if (!track)
            {
                destination.IsChanged = false;
            }
        }

        public static void UpdateSettings(UserSettings value)
        {
            UpdateSettings(value, true);
        }

        public static void UpdateSettings(UserSettings value, bool track)
        {
            if (value == null)
            {
                return;
            }

            ShallowCopy(value, Settings, track);

            ThemeHelper.RefreshTheme();
        }
    }
}