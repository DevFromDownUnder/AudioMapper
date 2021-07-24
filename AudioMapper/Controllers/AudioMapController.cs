using AudioMapper.Helpers;
using AudioMapper.Models;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using PropertyChanged;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMapController : IDisposable, IMMNotificationClient
    {
        private readonly MMDeviceEnumerator deviceEnumerator;
        private bool disposedValue;

        public AudioMapController()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        public ConcurrentObservableCollection<AudioMap> LiveMaps { get; internal set; } = new ConcurrentObservableCollection<AudioMap>();

        public bool Exists(Device origin, Device destination)
        {
            return ExistsByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        public bool ExistsByDeviceId(string originId, string destinationId)
        {
            return LiveMaps?.Any((m) => m.Origin?.DeviceId == originId && m.Destination?.DeviceId == destinationId) ?? false;
        }

        public AudioMap GetMap(Device origin, Device destination)
        {
            return GetMapByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        public AudioMap GetMapByDeviceId(string originId, string destinationId)
        {
            return LiveMaps?.FirstOrDefault((m) => m.Origin?.DeviceId == originId && m.Destination?.DeviceId == destinationId);
        }

        public void Remove(Device origin, Device destination)
        {
            RemoveMapByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        public void RemoveAllByDeviceId(string id)
        {
            var maps = LiveMaps?.Where((m) => m.Origin?.DeviceId == id || m.Destination?.DeviceId == id)?.ToList();

            foreach (AudioMap map in maps)
            {
                LiveMaps?.Remove(map);
                FunctionHelper.ConsumeExceptions(() => map.Dispose());
            }
        }

        public void RemoveMapByDeviceId(string originId, string destinationId)
        {
            List<AudioMap> maps = LiveMaps?.Where((m) => m.Origin?.DeviceId == originId && m.Destination?.DeviceId == destinationId)?.ToList();

            foreach (AudioMap map in maps)
            {
                LiveMaps?.Remove(map);
                FunctionHelper.ConsumeExceptions(() => map.Dispose());
            }
        }

        public void Start()
        {
            foreach (AudioMap map in LiveMaps)
            {
                FunctionHelper.ConsumeExceptions(() => map.Start());
            }
        }

        public void Stop()
        {
            foreach (AudioMap map in LiveMaps)
            {
                FunctionHelper.ConsumeExceptions(() => map.Stop());
            }
        }

        public void Upsert(Device origin, Device destination, int? latency = null)
        {
            //Check for live maps
            AudioMap current = GetMap(origin, destination);

            if (current != null)
            {
                //Mapping found
                current.Volume = origin.Volume;
            }
            else
            {
                using (MMDevice originDevice = deviceEnumerator.GetDevice(origin.DeviceId))
                {
                    using (MMDevice destinationDevice = deviceEnumerator.GetDevice(destination.DeviceId))
                    {
                        if (originDevice != null && destinationDevice != null)
                        {
                            LiveMaps?.Add(new AudioMap(origin, destination, originDevice, destinationDevice, latency));
                        }
                    }
                }
            }
        }

        #region ~AudioMapController

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Unregister from events
                    FunctionHelper.ConsumeExceptions(() => deviceEnumerator?.UnregisterEndpointNotificationCallback(this));

                    //Cleanup our maps and devices
                    foreach (AudioMap map in LiveMaps)
                    {
                        FunctionHelper.ConsumeExceptions(() => map.Dispose());
                    }
                }

                LiveMaps = null;
                FunctionHelper.ConsumeExceptions(() => deviceEnumerator?.Dispose());

                disposedValue = true;
            }
        }

        #endregion ~AudioMapController

        #region MMDeviceNotification handlers

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
        }

        public void OnDeviceRemoved(string deviceId)
        {
            RemoveAllByDeviceId(deviceId);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (newState != DeviceState.Active)
            {
                RemoveAllByDeviceId(deviceId);
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }

        #endregion MMDeviceNotification handlers
    }
}