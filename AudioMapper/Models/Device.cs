using NAudio.CoreAudioApi;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Device
    {
        public string DeviceId { get; set; } = string.Empty;
        public SoundDevices.DeviceType DeviceType { get; set; } = SoundDevices.DeviceType.Output;
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Device = Destination
        /// MappedDevices = Source
        /// Setup this way for easier display binding
        /// </summary>
        public ObservableCollection<Device> MappedDevices { get; set; } = new ObservableCollection<Device>();

        public SoundDevices.MapState MapState { get; set; } = SoundDevices.MapState.Inactive;
        public string Name { get; set; } = string.Empty;
        public SoundDevices.PendingAction PendingAction { get; set; } = SoundDevices.PendingAction.None;
        public float Volume { get; set; } = 1.0f;

        public static Device FromMMDevice(MMDevice source)
        {
            return new Device()
            {
                DeviceType = source.DataFlow == DataFlow.Capture ? SoundDevices.DeviceType.Input : SoundDevices.DeviceType.Output,
                DeviceId = source.ID,
                Name = source.FriendlyName
            };
        }

        public Device CopyForAction(SoundDevices.PendingAction action)
        {
            return new Device()
            {
                DeviceType = DeviceType,
                DeviceId = DeviceId,
                MapState = MapState,
                Name = Name,
                PendingAction = action,
                Volume = Volume
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}