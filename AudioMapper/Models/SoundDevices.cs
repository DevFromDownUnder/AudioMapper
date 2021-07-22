using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AudioMapper.Models
{
    public static class SoundDevices
    {
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

        public enum PendingAction
        {
            None,
            Add,
            Remove
        }
    }
}