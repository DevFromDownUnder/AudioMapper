using AudioMapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swordfish.NET.Collections;
using AudioMapper.Helpers;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using PropertyChanged;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMapController : IDisposable, IMMNotificationClient
    {
        private bool disposedValue;

        private readonly MMDeviceEnumerator deviceEnumerator;

        public AudioMapController()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        public ConcurrentObservableCollection<AudioMap> LiveMaps { get; internal set; } = new ConcurrentObservableCollection<AudioMap>();

        public void RemoveAllById(string id)
        {
            var maps = LiveMaps?.Where((m) => m.Origin?.Id == id || m.Destination?.Id == id)?.ToList();

            foreach (AudioMap map in maps)
            {
                LiveMaps?.Remove(map);
                FunctionHelper.ConsumeExceptions(() => map.Dispose());
            }
        }

        public void RemoveMapById(string originId, string destinationId)
        {
            List<AudioMap> maps = LiveMaps?.Where((m) => m.Origin?.Id == originId && m.Destination?.Id == destinationId)?.ToList();

            foreach (AudioMap map in maps)
            {
                LiveMaps?.Remove(map);
                FunctionHelper.ConsumeExceptions(() => map.Dispose());
            }
        }

        public void Remove(Device origin, Device destination)
        {
            RemoveMapById(origin.Id, destination.Id);
        }

        public bool ExistsById(string originId, string destinationId)
        {
            return LiveMaps?.Any((m) => m.Origin?.Id == originId && m.Destination?.Id == destinationId) ?? false;
        }

        public bool Exists(Device origin, Device destination)
        {
            return ExistsById(origin.Id, destination.Id);
        }

        public void Upsert(Device origin, Device destination, int? latency = null)
        {
            //Check for live maps
            AudioMap current = LiveMaps.FirstOrDefault((m) => m.Origin?.Id == origin.Id && m.Destination?.Id == destination.Id);

            if (current != null)
            {
                //Mapping found
                current.Volume = destination.Volume;
            }
            else
            {
                using (MMDevice originDevice = deviceEnumerator.GetDevice(origin.Id))
                {
                    using (MMDevice destinationDevice = deviceEnumerator.GetDevice(destination.Id))
                    {
                        if (originDevice != null && destinationDevice != null)
                        {
                            LiveMaps?.Add(new AudioMap(origin, destination, originDevice, destinationDevice, latency));
                        }
                    }
                }
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

        #region ~AudioMapController
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region MMDeviceNotification handlers
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (newState != DeviceState.Active)
            {
                RemoveAllById(deviceId);
            }
        }

        public void OnDeviceAdded(string pwstrDeviceId) { }

        public void OnDeviceRemoved(string deviceId)
        {
            RemoveAllById(deviceId);
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        #endregion
    }
}
