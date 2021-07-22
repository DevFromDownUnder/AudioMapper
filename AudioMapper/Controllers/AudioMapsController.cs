using AudioMapper.Helpers;
using AudioMapper.Models;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using PropertyChanged;
using System;
using System.Linq;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMapsController : IMMNotificationClient, IDisposable
    {
        private readonly MMDeviceEnumerator deviceEnumerator;
        private readonly AudioMaps deviceMaps;

        public AudioMapsController()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceMaps = new AudioMaps();
            Devices = GetSoundDevices();
        }

        public SoundDevices Devices { get; set; }

        public void Dispose()
        {
            Helper.ConsumeExceptions(() => deviceEnumerator?.Dispose());
            Helper.ConsumeExceptions(() => deviceMaps?.Dispose());
        }

        public void Map()
        {
            MMDeviceCollection systemDevices = GetAllActiveSystemDevices();

            foreach (Device destination in Devices?.Where(d => d.MappedDevices.Any()).ToList())
            {
                foreach (MappedDevice origin in destination.MappedDevices)
                {
                    using (MMDevice input = systemDevices?.FirstOrDefault(d => d.ID == origin.Id))
                    {
                        using (MMDevice output = systemDevices?.FirstOrDefault(d => d.ID == destination.Id))
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
            Devices?.AddMMDeviceIfNew(deviceEnumerator?.GetDevice(id));
        }

        private MMDeviceCollection GetAllActiveSystemDevices()
        {
            return deviceEnumerator?.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        }

        private SoundDevices GetSoundDevices()
        {
            return new SoundDevices(GetAllActiveSystemDevices()?.Select(d => Device.FromMMDevice(d)));
        }

        private void RemoveDeviceById(string id)
        {
            deviceMaps?.RemoveAllAudioMapsWithId(id);
            Devices?.RemoveAllUsagesOfDeviceById(id);
        }
    }
}