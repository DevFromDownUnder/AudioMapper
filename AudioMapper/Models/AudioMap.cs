using AudioMapper.Helpers;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;

namespace AudioMapper.Models
{
    public class AudioMap : IDisposable
    {
        public static int DEFAULT_LATENCY = 100;

        private float volume;

        public AudioMap(MMDevice origin, MMDevice destination, int? latency = null)
        {
            Latency = latency ?? DEFAULT_LATENCY;
            OriginDeviceID = origin.ID;
            DestinationDeviceID = destination.ID;

            switch (origin.DataFlow)
            {
                case DataFlow.Capture:
                    CaptureStream = new WasapiCapture(origin, true, Latency) { ShareMode = AudioClientShareMode.Shared };
                    break;

                case DataFlow.Render:
                    CaptureStream = new WasapiLoopbackCapture(origin) { ShareMode = AudioClientShareMode.Shared };
                    break;

                case DataFlow.All:
                    CaptureStream = new WasapiCapture(origin, true, Latency) { ShareMode = AudioClientShareMode.Shared };
                    break;
            }

            Buffer = new WaveInProvider(CaptureStream);
            PlaybackStream = new WasapiOut(destination, AudioClientShareMode.Shared, true, Latency);
            PlaybackStream.Init(Buffer);
        }

        public string DestinationDeviceID { get; set; }
        public int Latency { get; set; }
        public string OriginDeviceID { get; set; }

        public float Volume
        {
            get => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume : volume);
            set => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume = value : volume = value);
        }

        private WaveInProvider Buffer { get; set; }
        private IWaveIn CaptureStream { get; set; }
        private WasapiOut PlaybackStream { get; set; }

        public void Dispose()
        {
            Helper.ConsumeExceptions(() => Stop());
            Helper.ConsumeExceptions(() => CaptureStream?.Dispose());
            Helper.ConsumeExceptions(() => PlaybackStream?.Dispose());
        }

        public void Start()
        {
            CaptureStream?.StartRecording();
            PlaybackStream?.Play();

            if (PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing)
            {
                PlaybackStream.Volume = volume;
            }
        }

        public void Stop()
        {
            CaptureStream?.StopRecording();
            PlaybackStream?.Stop();
        }
    }
}