using NAudio.CoreAudioApi;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Device
    {
        public SoundDevices.DeviceType DeviceType { get; set; } = SoundDevices.DeviceType.Output;
        public string Id { get; set; } = string.Empty;
        public ObservableCollection<Device> MappedDevices { get; set; } = new ObservableCollection<Device>();
        public SoundDevices.MapState MapState { get; set; } = SoundDevices.MapState.Inactive;
        public SoundDevices.PendingAction PendingAction { get; set; } = SoundDevices.PendingAction.None;
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