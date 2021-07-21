#define DO_NOT_USE_CSCORE

#if USE_CSCORE
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using MMDevice = AudioMapper.Extensions.CSCoreExtensions.MMDevice;
#else

using NAudio.CoreAudioApi;
using NAudio.Wave;

#endif

using System;
using AudioMapper.Helpers;

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
#if USE_CSCORE
                    CaptureStream = new WasapiCapture(true, AudioClientShareMode.Shared, Latency) { Device = origin };
#else
                    CaptureStream = new WasapiCapture(origin, true, Latency) { ShareMode = AudioClientShareMode.Shared };
#endif
                    break;

                case DataFlow.Render:
#if USE_CSCORE
                    CaptureStream = new WasapiLoopbackCapture(Latency) { Device = origin };
#else
                    CaptureStream = new WasapiLoopbackCapture(origin) { ShareMode = AudioClientShareMode.Shared };
#endif
                    break;

                case DataFlow.All:
#if USE_CSCORE
                    CaptureStream = new WasapiCapture(true, AudioClientShareMode.Shared, Latency) { Device = origin };
#else
                    CaptureStream = new WasapiCapture(origin, true, Latency) { ShareMode = AudioClientShareMode.Shared };
#endif
                    break;
            }

#if USE_CSCORE
            CaptureStream.Initialize();
            Buffer = new SoundInSource(CaptureStream) { FillWithZeros = true };
            PlaybackStream = new WasapiOut() { Device = destination };

            PlaybackStream.Initialize(Buffer);
#else
            Buffer = new WaveInProvider(CaptureStream);
            PlaybackStream = new WasapiOut(destination, AudioClientShareMode.Shared, true, Latency);
            PlaybackStream.Init(Buffer);
#endif
        }

        public string DestinationDeviceID { get; set; }
        public int Latency { get; set; }
        public string OriginDeviceID { get; set; }

        public float Volume
        {
            get => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume : volume);
            set => Helper.ConsumeExceptions(() => PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing ? PlaybackStream.Volume = value : volume = value);
        }

#if USE_CSCORE
        private SoundInSource Buffer { get; set; }
        private ISoundIn CaptureStream { get; set; }
#else
        private WaveInProvider Buffer { get; set; }
        private IWaveIn CaptureStream { get; set; }
#endif
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