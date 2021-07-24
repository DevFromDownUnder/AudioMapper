using AudioMapper.Helpers;
using AudioMapper.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioMapper.Converters
{
    public class DeviceColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values.Length == 3)
                {
                    SoundDevices.DeviceType? deviceType = values[0] as SoundDevices.DeviceType?;
                    SoundDevices.MapState? mapState = values[1] as SoundDevices.MapState?;
                    SoundDevices.PendingAction? pendingAction = values[2] as SoundDevices.PendingAction?;

                    if (deviceType != null && mapState != null && pendingAction != null)
                    {
                        switch (pendingAction)
                        {
                            case SoundDevices.PendingAction.Add:
                                return new SolidColorBrush(Colors.Lime);

                            case SoundDevices.PendingAction.Remove:
                                return new SolidColorBrush(Colors.Red);

                            case SoundDevices.PendingAction.None:
                                if (mapState == SoundDevices.MapState.Inactive)
                                {
                                    return new SolidColorBrush(ThemeHelper.DefaultForegroundColor);
                                }
                                else
                                {
                                    return new SolidColorBrush(SettingsHelper.Settings.Theme_PrimaryColor);
                                }
                        }
                    }
                }
            }
            catch { }

            return new SolidColorBrush(ThemeHelper.DefaultForegroundColor);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}