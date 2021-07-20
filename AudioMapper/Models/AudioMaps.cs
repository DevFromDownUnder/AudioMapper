using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AudioMapper.Models
{
    public class AudioMaps : ObservableCollection<AudioMap>, IDisposable
    {
        public AudioMaps(IEnumerable<AudioMap> collection) : base(collection) { }
        public AudioMaps() : base() { }

        public void UpdateMap(MMDevice origin, MMDevice destination, float volume = 1.0f, int? latency = null)
        {
            AudioMap current = Items?.FirstOrDefault((map) => map?.OriginDeviceID == origin?.DeviceID && map?.DestinationDeviceID == destination?.DeviceID);

            if (current != null)
            {
                current.Volume = volume;
            }
            else
            {
                current = new AudioMap(origin, destination, latency)
                {
                    Volume = volume
                };

                Add(current);
            }
        }

        public void RemoveAudioMapById(string originId, string destinationId)
        {
            List<AudioMap> maps = Items?.Where((m) => m.OriginDeviceID == originId && m.DestinationDeviceID == destinationId)?.ToList();

            foreach (AudioMap map in maps)
            {
                Helper.ConsumeExceptions(() => map.Stop());
                Items.Remove(map);
            }
        }

        public void RemoveAllAudioMapsWithId(string id)
        {
            var maps = Items?.Where((m) => m.OriginDeviceID == id || m.DestinationDeviceID == id)?.ToList();

            foreach (AudioMap map in maps)
            {
                Helper.ConsumeExceptions(() => map.Stop());
                Items.Remove(map);
            }
        }

        public void Start()
        {
            foreach (AudioMap item in Items)
            {
                Helper.ConsumeExceptions(() => item.Start());
            }
        }
        public void Stop()
        {
            foreach (AudioMap item in Items)
            {
                Helper.ConsumeExceptions(() => item.Stop());
            }
        }

        public void Dispose()
        {
            foreach (var map in Items)
            {
                map.Dispose();
            }
        }
    }
}
