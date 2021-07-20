using CSCore.CoreAudioAPI;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Device
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public SoundDevices.DeviceType DeviceType { get; set; } = SoundDevices.DeviceType.Output;
        public float Volume { get; set; } = 1.0f;
        public SoundDevices.MapState MapState { get; set; } = SoundDevices.MapState.Inactive;
        public ObservableCollection<MappedDevice> MappedDevices { get; set; } = new ObservableCollection<MappedDevice>();

        public static Device FromMMDevice(MMDevice source)
        {
            return new Device()
            {
                DeviceType = source.DataFlow == DataFlow.Capture ? SoundDevices.DeviceType.Input : SoundDevices.DeviceType.Output,
                Id = source.DeviceID,
                Name = source.FriendlyName
            };
        }

        public override string ToString() => Name;
    }
}
