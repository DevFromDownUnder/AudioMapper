using AudioMapper.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AudioMapper.Converters
{
    public class DeviceImageConverter : IMultiValueConverter
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
                        if (deviceType == SoundDevices.DeviceType.Output)
                        {
                            switch (pendingAction)
                            {
                                case SoundDevices.PendingAction.Add:
                                    return PackIconKind.VolumePlus;

                                case SoundDevices.PendingAction.Remove:
                                    return PackIconKind.VolumeMinus;

                                case SoundDevices.PendingAction.None:
                                    return PackIconKind.VolumeHigh;
                            }
                        }
                        else
                        {
                            switch (pendingAction)
                            {
                                case SoundDevices.PendingAction.Add:
                                    return PackIconKind.MicrophoneAdd;

                                case SoundDevices.PendingAction.Remove:
                                    return PackIconKind.MicrophoneMinus;

                                case SoundDevices.PendingAction.None:
                                    return PackIconKind.Microphone;
                            }
                        }
                    }
                }
            }
            catch { }

            return PackIconKind.VolumeHigh;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}