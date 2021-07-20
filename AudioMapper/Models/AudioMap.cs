using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using System;

namespace AudioMapper.Models
{
    public class AudioMap : IDisposable
    {
        public static int DEFAULT_LATENCY = 100;

        private float volume;

        public string OriginDeviceID { get; set; }
        public string DestinationDeviceID { get; set; }
        public int Latency { get; set; }
        private ISoundIn CaptureStream { get; set; }
        private SoundInSource Buffer { get; set; }
        private WasapiOut PlaybackStream { get; set; }

        public float Volume
        {
            get => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume : volume);
            set => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume = value : volume = value);
        }

        public AudioMap(MMDevice origin, MMDevice destination, int? latency = null)
        {
            Latency = latency ?? DEFAULT_LATENCY;
            OriginDeviceID = origin.DeviceID;
            DestinationDeviceID = destination.DeviceID;

            switch (origin.DataFlow)
            {
                case DataFlow.Capture:
                    CaptureStream = new WasapiCapture(true, AudioClientShareMode.Shared, Latency) { Device = origin };
                    break;
                case DataFlow.Render:
                    CaptureStream = new WasapiLoopbackCapture(Latency) { Device = origin };
                    break;
            }

            CaptureStream.Initialize();

            Buffer = new SoundInSource(CaptureStream) { FillWithZeros = true };
            PlaybackStream = new WasapiOut() { Device = destination };

            PlaybackStream.Initialize(Buffer);
        }

        public void Start()
        {
            CaptureStream?.Start();
            PlaybackStream?.Play();

            if (PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing)
            {
                PlaybackStream.Volume = volume;
            }
        }
        public void Stop()
        {
            CaptureStream?.Stop();
            PlaybackStream?.Stop();
        }

        public void Dispose()
        {
            Helper.ConsumeExceptions(() => Stop());
            Helper.ConsumeExceptions(() => CaptureStream?.Dispose());
            Helper.ConsumeExceptions(() => PlaybackStream?.Dispose());
        }
    }
}
