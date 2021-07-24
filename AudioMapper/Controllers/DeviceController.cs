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

namespace AudioMapper.Controllers
{
    [AddINotifyPropertyChangedInterface]
    public class DeviceController : IDisposable, IMMNotificationClient
    {
        private readonly AudioMapController controller;
        private readonly MMDeviceEnumerator deviceEnumerator;
        private bool disposedValue;

        public DeviceController()
        {
            controller = new AudioMapController();
            deviceEnumerator = new MMDeviceEnumerator();
            InitializeDevices();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        public ConcurrentObservableSortedCollection<Device> Devices { get; internal set; } = new ConcurrentObservableSortedCollection<Device>(new DeviceComparer());

        public AudioMapController MapController
        {
            get
            {
                return controller;
            }
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
            destination?.MappedDevices?.Add(origin.CopyForAction(SoundDevices.PendingAction.Add));
        }

        public bool CanMap(Device origin, Device destination)
        {
            return destination.DeviceType != SoundDevices.DeviceType.Input &&
                   origin.DeviceId != destination.DeviceId &&
                   !Exists(origin, destination);
        }

        public bool DestinationDeviceExistsById(Guid Id)
        {
            return Devices?.Any((d) => d.Id == Id) ?? false;
        }

        public bool Exists(Device origin, Device destination)
        {
            return ExistsByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        public bool ExistsByDeviceId(string originId, string destinationId)
        {
            return Devices?.Any((d) => d.DeviceId == destinationId && (d.MappedDevices?.Any((m) => m.DeviceId == originId) ?? false)) ?? false;
        }

        public Device GetDestinationDeviceById(Guid Id)
        {
            return Devices?.FirstOrDefault((d) => d.Id == Id);
        }

        public Device GetDestinationDeviceBySourceDeviceId(Guid Id)
        {
            return Devices?.FirstOrDefault((d) => d.MappedDevices?.Any((m) => m.Id == Id) ?? false);
        }

        public Device GetSourceDeviceById(Guid Id)
        {
            return Devices?.FirstOrDefault((d) => d.MappedDevices?.Any((m) => m.Id == Id) ?? false)?.MappedDevices?.FirstOrDefault((m) => m.Id == Id);
        }

        public void InitializeDevices()
        {
            MMDeviceCollection devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

            foreach (MMDevice device in devices)
            {
                AddDevice(device);
            }
        }

        public void PushMapsToLive(int? latencey = null)
        {
            var proposedMaps = Devices?.SelectMany((d) => d?.MappedDevices?.Select((m) => (m, d)));
            List<(string, string)> lstMapsToRemove = new List<(string, string)>();

            foreach (var (origin, destination) in proposedMaps)
            {
                switch (origin?.PendingAction)
                {
                    case SoundDevices.PendingAction.Add:
                        //Update origin statuses
                        origin.MapState = SoundDevices.MapState.Active;
                        origin.PendingAction = SoundDevices.PendingAction.None;

                        //Update destination statuses
                        destination.MapState = SoundDevices.MapState.Active;
                        destination.PendingAction = SoundDevices.PendingAction.None;

                        controller?.Upsert(origin, destination, latencey);
                        break;

                    case SoundDevices.PendingAction.Remove:
                        //Can't remove while looping through
                        lstMapsToRemove.Add((origin.DeviceId, destination.DeviceId));

                        controller?.Remove(origin, destination);
                        break;

                    case SoundDevices.PendingAction.None:
                        controller?.Upsert(origin, destination, latencey);
                        break;
                }
            }

            foreach (var (originId, destinationID) in lstMapsToRemove)
            {
                RemoveMapByDeviceId(originId, destinationID);
            }
        }

        /// <summary>
        /// Removed regardless of live mappings
        /// </summary>
        public void RemoveAllByDeviceId(string id)
        {
            //Remove root devices
            var devices = Devices?.Where((d) => d.DeviceId == id)?.ToList();

            foreach (Device device in devices)
            {
                Devices?.Remove(device);
            }

            //Remove mapped devices
            foreach (Device device in Devices)
            {
                var mappedDevices = device.MappedDevices?.Where((m) => m.DeviceId == id)?.ToList();

                foreach (Device mappedDevice in mappedDevices)
                {
                    device?.MappedDevices?.Remove(mappedDevice);
                }
            }
        }

        /// <summary>
        /// Removes mapping regardless of live mapping
        /// </summary>
        public void RemoveMap(Device origin, Device destination)
        {
            RemoveMapByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        /// <summary>
        /// Removes mapping regardless of live mapping
        /// </summary>
        public void RemoveMapByDeviceId(string originId, string destinationId)
        {
            var devices = Devices?.Where((d) => d.DeviceId == destinationId)?.ToList();

            foreach (Device device in devices)
            {
                //Remove mapped devices
                var mappedDevices = device.MappedDevices?.Where((m) => m.DeviceId == originId)?.ToList();

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

        public bool SourceExistsDeviceById(Guid Id)
        {
            return Devices?.Any((d) => d.MappedDevices?.Any((m) => m.Id == Id) ?? false) ?? false;
        }

        public void Start()
        {
            controller?.Start();
        }

        public void Stop()
        {
            controller?.Stop();
        }

        /// <summary>
        /// Changes state if exists in live mappings, removed otherwise
        /// </summary>
        public void UpRemoveProposedMap(Device origin, Device destination)
        {
            UpRemoveProposedMapByDeviceId(origin.DeviceId, destination.DeviceId);
        }

        /// <summary>
        /// Changes state if exists in live mappings, removed otherwise
        /// </summary>
        public void UpRemoveProposedMapByDeviceId(string originId, string destinationId)
        {
            var liveMapExists = controller?.ExistsByDeviceId(originId, destinationId) ?? false;

            var devices = Devices?.Where((d) => d.DeviceId == destinationId)?.ToList();

            foreach (Device device in devices)
            {
                //Remove mapped devices
                var mappedDevices = device.MappedDevices?.Where((m) => m.DeviceId == originId)?.ToList();

                foreach (Device mappedDevice in mappedDevices)
                {
                    if (liveMapExists)
                    {
                        if (mappedDevice.PendingAction == SoundDevices.PendingAction.Remove)
                        {
                            mappedDevice.PendingAction = SoundDevices.PendingAction.None;
                        }
                        else
                        {
                            mappedDevice.PendingAction = SoundDevices.PendingAction.Remove;
                        }
                    }
                    else
                    {
                        device?.MappedDevices?.Remove(mappedDevice);
                    }
                }
            }
        }

        #region ~DeviceController

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
                    FunctionHelper.ConsumeExceptions(() => controller?.Dispose());
                }

                Devices = null;
                FunctionHelper.ConsumeExceptions(() => deviceEnumerator?.Dispose());

                disposedValue = true;
            }
        }

        #endregion ~DeviceController

        #region MMDeviceNotification handlers

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            AddDevice(pwstrDeviceId);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            RemoveAllByDeviceId(deviceId);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            if (newState == DeviceState.Active)
            {
                AddDevice(deviceId);
            }
            else
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