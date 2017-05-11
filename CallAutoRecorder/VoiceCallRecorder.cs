using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Java.IO;

namespace CallAutoRecorder
{
    class VoiceCallRecorder : IDisposable
    {
        public MediaRecorder _Recorder { get; set; }

        public bool IsRec { get; private set; }

        public event EventHandler<RecFinishedArgs> RecFinished;

        private string _OutputPath { get; set; }

        public VoiceCallRecorder()
        {
            //https://developer.android.com/reference/android/media/MediaRecorder.html
            _Recorder = new MediaRecorder();

            _Recorder.SetAudioSource(AudioSource.VoiceCall);
            //API requirements
            //Container : WAV
            //Encoding : PCM
            //Rate : 16K
            //Sample Format : 16bit
            //Channels : Mono

            //AndroidはWAV対応してないようなので自前で組み込む必要があるとのこと
            //http://stackoverflow.com/questions/4871149/how-to-record-voice-in-wav-format-in-android

            _Recorder.SetOutputFormat(OutputFormat.Mpeg4);
            _Recorder.SetAudioEncoder(AudioEncoder.Aac);

            var storage = File.CreateTempFile("hoge_", ".mp4", Android.OS.Environment.ExternalStorageDirectory);
            _OutputPath = storage.AbsolutePath;

            _Recorder.SetOutputFile(_OutputPath);
            _Recorder.Prepare();
        }
        public void RecStart()
        {
            if (IsRec) return;

            _Recorder.Start();
            IsRec = true;
        }

        public void RecStop(string number)
        {
            if (!IsRec) return;

            _Recorder.Stop();
            IsRec = false;
            
            RecFinished?.Invoke(this, new RecFinishedArgs
            {
                RecAudioPath = _OutputPath,
                IncomingNumber = number,
            });
        }

        public void Dispose()
        {
            if(_Recorder != null)
            {
                _Recorder.Release();
                _Recorder = null;
            }
        }
    }

    public class RecFinishedArgs : EventArgs
    {
        public string RecAudioPath { get; set; }

        public string IncomingNumber { get; set; }
    }
}