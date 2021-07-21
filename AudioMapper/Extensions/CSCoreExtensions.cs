#define DO_NOT_USE_CSCORE

#if USE_CSCORE
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using System;
#endif

namespace AudioMapper.Extensions
{
#if USE_CSCORE
    public static class CSCoreExtensions
    {
        public class MMDevice : CSCore.CoreAudioAPI.MMDevice
        {
            public MMDevice(IntPtr ptr) : base(ptr)
            {
            }

            public string ID => DeviceID;
        }

        public static MMDeviceCollection EnumerateAudioEndPoints(this MMDeviceEnumerator enumerator, DataFlow dataFlow, DeviceState stateMask)
        {
            return enumerator.EnumAudioEndpoints(dataFlow, stateMask);
        }

        public static void StartRecording(this ISoundIn source)
        {
            source.Start();
        }
        public static void StopRecording(this ISoundIn source)
        {
            source.Stop();
        }
    }
#endif
}