#define DO_NOT_USE_CSCORE

#if USE_CSCORE
using CSCore.CoreAudioAPI;
using CSCore.Win32;
using MMDevice = AudioMapper.Extensions.CSCoreExtensions.MMDevice;
#else

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

#endif

using AudioMapper.Models;
using AudioMapper.Helpers;
using PropertyChanged;
using System.Linq;
using System;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMapController : IMMNotificationClient, IDisposable
    {
        private readonly MMDeviceEnumerator deviceEnumerator;
        private readonly AudioMaps deviceMaps;

        public AudioMapController()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceMaps = new AudioMaps();
            Devices = GetSoundDevices();
        }

        public SoundDevices Devices { get; set; }

        public void Map()
        {
#if USE_CSCORE
            using (MMDeviceCollection systemDevices = GetAllActiveSystemDevices())
            {
#else
            MMDeviceCollection systemDevices = GetAllActiveSystemDevices();
#endif

            foreach (Device destination in Devices?.Where(d => d.MappedDevices.Any()).ToList())
            {
                foreach (MappedDevice origin in destination.MappedDevices)
                {
                    using (MMDevice input = (MMDevice)systemDevices?.FirstOrDefault(d => ((MMDevice)d).ID == origin.Id))
                    {
                        using (MMDevice output = (MMDevice)systemDevices?.FirstOrDefault(d => ((MMDevice)d).ID == destination.Id))
                        {
                            if (input == null || output == null)
                            {
                                continue;
                            }

                            origin.MapState = SoundDevices.MapState.Active;
                            destination.MapState = SoundDevices.MapState.Active;

                            deviceMaps?.UpdateMap(input, output, origin.Volume);
                        }
                    }
                }
            }
#if USE_CSCORE
            }
#endif
        }

        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow dataFlow, Role role, string deviceId)
        {
            Helper.NothingButMemes();
        }

        void IMMNotificationClient.OnDeviceAdded(string deviceId)
        {
            AddDeviceIfNewById(deviceId);
        }

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
        {
            RemoveDeviceById(deviceId);
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

        void IMMNotificationClient.OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            Helper.NothingButMemes();
        }

        public void RemoveMapIfExists(string originId, string destinationId)
        {
            deviceMaps?.RemoveAudioMapById(originId, destinationId);
        }

        public void Start()
        {
            deviceMaps?.Start();
        }

        public void Stop()
        {
            deviceMaps?.Stop();
        }

        private void AddDeviceIfNewById(string id)
        {
            Devices?.AddMMDeviceIfNew((MMDevice)deviceEnumerator?.GetDevice(id));
        }

        private MMDeviceCollection GetAllActiveSystemDevices()
        {
            return deviceEnumerator?.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        }

        private SoundDevices GetSoundDevices()
        {
            return new SoundDevices(GetAllActiveSystemDevices()?.Select(d => Device.FromMMDevice((MMDevice)d)));
        }

        private void RemoveDeviceById(string id)
        {
            deviceMaps?.RemoveAllAudioMapsWithId(id);
            Devices?.RemoveAllUsagesOfDeviceById(id);
        }

        public void Dispose()
        {
            Helper.ConsumeExceptions(() => deviceEnumerator?.Dispose());
            Helper.ConsumeExceptions(() => deviceMaps?.Dispose());
        }
    }
}