using CSCore.CoreAudioAPI;
using CSCore.Win32;
using AudioMapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using PropertyChanged;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMapController : IMMNotificationClient
    {
        private readonly AudioMaps deviceMaps;
        private readonly MMDeviceEnumerator deviceEnumerator;

        public SoundDevices Devices { get; set; }

        public AudioMapController()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceMaps = new AudioMaps();
            Devices = GetSoundDevices();
        }

        private MMDeviceCollection GetAllActiveSystemDevices()
        {
            return deviceEnumerator.EnumAudioEndpoints(DataFlow.All, DeviceState.Active);
        }

        private SoundDevices GetSoundDevices()
        {
            return new SoundDevices(GetAllActiveSystemDevices()?.Select(d => Device.FromMMDevice(d)));
        }

        public void Map()
        {
            using (MMDeviceCollection systemDevices = GetAllActiveSystemDevices())
            {
                foreach (Device destination in Devices.Where(d => d.MappedDevices.Any()).ToList())
                {
                    foreach (MappedDevice origin in destination.MappedDevices)
                    {
                        using (MMDevice input = systemDevices.FirstOrDefault(d => d.DeviceID == origin.Id))
                        {
                            using (MMDevice output = systemDevices.FirstOrDefault(d => d.DeviceID == destination.Id))
                            {
                                if (input == null || output == null)
                                {
                                    continue;
                                }

                                origin.MapState = SoundDevices.MapState.Active;
                                destination.MapState = SoundDevices.MapState.Active;

                                deviceMaps.UpdateMap(input, output, origin.Volume);
                            }
                        }
                    }
                }
            }
        }

        public void Start()
        {
            deviceMaps?.Start();
        }

        public void Stop()
        {
            deviceMaps?.Stop();
        }

        public void RemoveMapIfExists(string originId, string destinationId)
        {
            deviceMaps?.RemoveAudioMapById(originId, destinationId);
        }

        private void AddDeviceIfNewById(string id)
        {
            Devices?.AddMMDeviceIfNew(deviceEnumerator.GetDevice(id));
        }

        private void RemoveDeviceById(string id)
        {
            deviceMaps?.RemoveAllAudioMapsWithId(id);
            Devices?.RemoveAllUsagesOfDeviceById(id);
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState deviceState)
        {
            switch (deviceState)
            {
                case DeviceState.Active:
                    AddDeviceIfNewById(deviceId);
                    break;
                default:
                    RemoveDeviceById(deviceId);
                    break;
            }
        }

        void IMMNotificationClient.OnDeviceAdded(string deviceId)
        {
            AddDeviceIfNewById(deviceId);
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            RemoveDeviceById(deviceId);
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow dataFlow, Role role, string deviceId)
        {
            Helper.NothingButMemes();
        }

        void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            Helper.NothingButMemes();
        }
    }
}
