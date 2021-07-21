#define DO_NOT_USE_CSCORE

#if USE_CSCORE
using CSCore.CoreAudioAPI;
using MMDevice = AudioMapper.Extensions.CSCoreExtensions.MMDevice;
#else

using NAudio.CoreAudioApi;

#endif

using PropertyChanged;
using System.Collections.ObjectModel;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Device
    {
        public SoundDevices.DeviceType DeviceType { get; set; } = SoundDevices.DeviceType.Output;
        public string Id { get; set; } = string.Empty;
        public ObservableCollection<MappedDevice> MappedDevices { get; set; } = new ObservableCollection<MappedDevice>();
        public SoundDevices.MapState MapState { get; set; } = SoundDevices.MapState.Inactive;
        public string Name { get; set; } = string.Empty;
        public float Volume { get; set; } = 1.0f;

        public static Device FromMMDevice(MMDevice source)
        {
            return new Device()
            {
                DeviceType = source.DataFlow == DataFlow.Capture ? SoundDevices.DeviceType.Input : SoundDevices.DeviceType.Output,
                Id = source.ID,
                Name = source.FriendlyName
            };
        }

        public override string ToString() => Name;
    }
}