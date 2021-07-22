using AudioMapper.Helpers;
using AudioMapper.Logic;
using AudioMapper.Models;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using PropertyChanged;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class DeviceController : IDisposable, IMMNotificationClient
    {
        private bool disposedValue;

        private readonly MMDeviceEnumerator deviceEnumerator;
        private readonly AudioMapController controller;

        public DeviceController()
        {
            controller = new AudioMapController();
            deviceEnumerator = new MMDeviceEnumerator();
            InitializeDevices();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        public void InitializeDevices()
        {
            MMDeviceCollection devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

            foreach (MMDevice device in devices)
            {
                AddDevice(device);
            }
        }

        public ConcurrentObservableSortedCollection<Device> Devices { get; internal set; } = new ConcurrentObservableSortedCollection<Device>(new DeviceComparer());

        public AudioMapController MapController
        {
            get
            {
                return controller;
            }
        }

        /// <summary>
        /// Removed regardless of live mappings
        /// </summary>
        public void RemoveAllById(string id)
        {
            //Remove root devices
            var devices = Devices?.Where((d) => d.Id == id)?.ToList();

            foreach (Device device in devices)
            {
                Devices?.Remove(device);
            }

            //Remove mapped devices
            foreach (Device device in Devices)
            {
                var mappedDevices = device.MappedDevices?.Where((m) => m.Id == id)?.ToList();

                foreach (Device mappedDevice in mappedDevices)
                {
                    device?.MappedDevices?.Remove(mappedDevice);
                }
            }
        }

        /// <summary>
        /// Changes state if exists in live mappings, removed otherwise
        /// </summary>
        public void RemoveProposedMapById(string originId, string destinationId)
        {
            var liveMapExists = controller?.ExistsById(originId, destinationId) ?? false;

            var devices = Devices?.Where((d) => d.Id == originId)?.ToList();

            foreach (Device device in devices)
            {
                //Remove mapped devices
                var mappedDevices = device.MappedDevices?.Where((m) => m.Id == destinationId)?.ToList();

                foreach (Device mappedDevice in mappedDevices)
                {
                    if (liveMapExists)
                    {
                        mappedDevice.PendingAction = SoundDevices.PendingAction.Remove;
                    }
                    else
                    {
                        device?.MappedDevices?.Remove(mappedDevice);
                    }
                }
            }
        }

        /// <summary>
        /// Changes state if exists in live mappings, removed otherwise
        /// </summary>
        public void RemoveProposedMap(Device origin, Device destination)
        {
            RemoveProposedMapById(origin.Id, destination.Id);
        }

        /// <summary>
        /// Removes mapping regardless of live mapping
        /// </summary>
        public void RemoveMapById(string originId, string destinationId)
        {
            var devices = Devices?.Where((d) => d.Id == originId)?.ToList();

            foreach (Device device in devices)
            {
                //Remove mapped devices
                var mappedDevices = device.MappedDevices?.Where((m) => m.Id == destinationId)?.ToList();

                foreach (Device mappedDevice in mappedDevices)
                {
                    device?.MappedDevices?.Remove(mappedDevice);
                }

                if ((device?.MappedDevices?.Count ?? 1) == 0)
                {
                    //No maps left, update the status
                    device.MapState = SoundDevices.MapState.Inactive;
                }
            }
        }

        /// <summary>
        /// Removes mapping regardless of live mapping
        /// </summary>
        public void RemoveMap(Device origin, Device destination)
        {
            RemoveMapById(origin.Id, destination.Id);
        }

        public bool CanMap(Device origin, Device destination)
        {
            return origin.DeviceType != SoundDevices.DeviceType.Input &&
                   origin.Id != destination.Id &&
                   !Exists(origin, destination);
        }

        public bool ExistsById(string originId, string destinationId)
        {
            return Devices?.Any((d) => d.Id == originId && (d.MappedDevices?.Any((m) => m.Id == destinationId) ?? false)) ?? false;
        }

        public bool Exists(Device origin, Device destination)
        {
            return ExistsById(origin.Id, destination.Id);
        }

        public void AddDevice(Device device)
        {
            Devices?.Add(device);
        }

        public void AddDevice(MMDevice device)
        {
            AddDevice(Device.FromMMDevice(device));
        }

        public void AddDevice(string deviceId)
        {
            using (MMDevice device = deviceEnumerator?.GetDevice(deviceId))
            {
                if (device != null)
                {
                    AddDevice(Device.FromMMDevice(device));
                }
            }
        }

        public void AddMap(Device origin, Device destination)
        {
            destination.PendingAction = SoundDevices.PendingAction.Add;
            origin?.MappedDevices?.Add(destination);
        }

        public void Start()
        {
            controller?.Start();
        }

        public void Stop()
        {
            controller?.Stop();
        }

        public void PushMapsToLive(int? latencey = null)
        {
            var proposedMaps = Devices?.SelectMany((d) => d?.MappedDevices?.Select((m) => (d, m)));
            List<(string, string)> lstMapsToRemove = new List<(string, string)>();

            foreach (var (origin, destination) in proposedMaps)
            {
                if (destination?.PendingAction == SoundDevices.PendingAction.Add)
                {
                    //Update origin statuses
                    origin.MapState = SoundDevices.MapState.Active;
                    origin.PendingAction = SoundDevices.PendingAction.None;

                    //Update destination statuses
                    destination.MapState = SoundDevices.MapState.Active;
                    destination.PendingAction = SoundDevices.PendingAction.None;

                    controller?.Upsert(origin, destination, latencey);
                }

                if (destination?.PendingAction == SoundDevices.PendingAction.Remove)
                {
                    //Can't remove while looping through
                    lstMapsToRemove.Add((origin.Id, destination.Id));

                    controller?.Remove(origin, destination);
                }
            }

            foreach (var (originId, destinationID) in lstMapsToRemove)
            {
                RemoveMapById(originId, destinationID);
            }
        }

        #region ~DeviceController
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Unregister from events
                    FunctionHelper.ConsumeExceptions(() => deviceEnumerator?.UnregisterEndpointNotificationCallback(this));

                    //Cleanup our maps and devices
                    FunctionHelper.ConsumeExceptions(() => controller?.Dispose());
                }

                Devices = null;
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
            if (newState == DeviceState.Active)
            {
                AddDevice(deviceId);
            }
            else
            {
                RemoveAllById(deviceId);
            }
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            AddDevice(pwstrDeviceId);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            RemoveAllById(deviceId);
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId) { }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
        #endregion
    }
}
