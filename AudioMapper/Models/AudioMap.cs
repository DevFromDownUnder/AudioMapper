using AudioMapper.Helpers;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PropertyChanged;
using System;

namespace AudioMapper.Models
{
    [AddINotifyPropertyChangedInterface]
    public class AudioMap : IDisposable
    {
        public static int DEFAULT_LATENCY = 100;

        private float volume;

        public AudioMap(Device origin, Device destination, MMDevice originDevice, MMDevice destinationDevice, int? latency = null)
        {
            Latency = latency ?? DEFAULT_LATENCY;
            Origin = origin;
            Destination = destination;

            volume = origin.Volume;

            switch (originDevice.DataFlow)
            {
                case DataFlow.Capture:
                    CaptureStream = new WasapiCapture(originDevice, true, Latency) { ShareMode = AudioClientShareMode.Shared };
                    break;

                case DataFlow.Render:
                    CaptureStream = new WasapiLoopbackCapture(originDevice) { ShareMode = AudioClientShareMode.Shared };
                    break;

                case DataFlow.All:
                    CaptureStream = new WasapiCapture(originDevice, true, Latency) { ShareMode = AudioClientShareMode.Shared };
                    break;
            }

            Buffer = new WaveInProvider(CaptureStream);
            PlaybackStream = new WasapiOut(destinationDevice, AudioClientShareMode.Shared, true, Latency);
            PlaybackStream.Init(Buffer);
        }

        public Device Destination { get; set; }
        public int Latency { get; set; }
        public Device Origin { get; set; }

        public float Volume
        {
            get => volume;
            set
            {
                FunctionHelper.ConsumeExceptions(() =>
                {
                    volume = value;

                    if (PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing)
                    {
                        PlaybackStream.Volume = value;
                    }
                });
            }
        }

        private WaveInProvider Buffer { get; set; }
        private IWaveIn CaptureStream { get; set; }
        private WasapiOut PlaybackStream { get; set; }

        public void Dispose()
        {
            FunctionHelper.ConsumeExceptions(() => Stop());
            FunctionHelper.ConsumeExceptions(() => CaptureStream?.Dispose());
            FunctionHelper.ConsumeExceptions(() => PlaybackStream?.Dispose());
        }

        public void Start()
        {
            if (PlaybackStream?.PlaybackState != PlaybackState.Playing)
            {
                //Only restart playing if we were stopped
                CaptureStream?.StartRecording();
                PlaybackStream?.Play();
            }

            if (PlaybackStream != null && PlaybackStream.PlaybackState == PlaybackState.Playing)
            {
                PlaybackStream.Volume = volume;
            }
        }

        public void Stop()
        {
            if (PlaybackStream?.PlaybackState != PlaybackState.Playing)
            {
                //Only stop playing if we were playing
                CaptureStream?.StopRecording();
                PlaybackStream?.Stop();
            }
        }
    }
}