using CSCore.CoreAudioAPI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AudioMapper.Models
{
    public class SoundDevices : ObservableCollection<Device>
    {
        public SoundDevices(IEnumerable<Device> collection) : base(collection)
        {
        }

        public SoundDevices() : base()
        {
        }

        public enum DeviceType
        {
            Input,
            Output
        }

        public enum MapState
        {
            Active,
            Inactive
        }

        public void AddMMDeviceIfNew(MMDevice newDevice)
        {
            if (newDevice != null)
            {
                if (!Items.Any(d => d.Id == newDevice?.DeviceID))
                {
                    Items.Add(Device.FromMMDevice(newDevice));
                }
            }
        }

        public void RemoveAllUsagesOfDeviceById(string id)
        {
            foreach (Device item in Items)
            {
                foreach (MappedDevice mappedItem in item.MappedDevices)
                {
                    if (mappedItem.Id == id)
                    {
                        item.MappedDevices.Remove(mappedItem);
                    }
                }

                Items.Remove(item);
            }
        }
    }
}