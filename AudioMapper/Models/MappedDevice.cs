using PropertyChanged;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class MappedDevice
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public SoundDevices.DeviceType DeviceType { get; set; } = SoundDevices.DeviceType.Output;
        public float Volume { get; set; } = 1.0f;
        public SoundDevices.MapState MapState { get; set; } = SoundDevices.MapState.Inactive;

        public static MappedDevice FromDevice(Device source)
        {
            return new MappedDevice()
            {
                Name = source.Name,
                Id = source.Id,
                DeviceType = source.DeviceType,
                Volume = source.Volume
            };
        }

        public override string ToString() => Name;
    }
}
