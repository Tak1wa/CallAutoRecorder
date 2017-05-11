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
using Android.Util;
using Android.Telephony;

namespace CallAutoRecorder
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class IncomingCallReceiver : BroadcastReceiver
    {
        public TelephonyManager _Manager { get; set; }
        public override void OnReceive(Context context, Intent intent)
        {
            _Manager = context.GetSystemService(Context.TelephonyService) as TelephonyManager;
            if (_Manager == null)
                return;

            var listener = new PhoneStateListenerEx(context);
            _Manager.Listen(listener, PhoneStateListenerFlags.CallState);
        }

        private class PhoneStateListenerEx : PhoneStateListener
        {
            private Context _Context { get; set; }
            private VoiceCallRecorder _Recorder { get; set; }
            public PhoneStateListenerEx(Context context)
            {
                _Context = context;

                try
                {
                    _Recorder = new VoiceCallRecorder();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(context, ex.ToString(), ToastLength.Long).Show();
                }

                _Recorder.RecFinished += (sender, e) =>
                {
                    //TODO:録音終了
                    Toast.MakeText(_Context, e.IncomingNumber, ToastLength.Short).Show();
                    Toast.MakeText(_Context, $"音声ファイル保存先：{e.RecAudioPath}", ToastLength.Short).Show();
                };
            }

            public override void OnCallStateChanged([GeneratedEnum] CallState state, string incomingNumber)
            {
                if(state == CallState.Offhook)
                {
                    if (_Recorder != null)
                    {
                        _Recorder.RecStart();
                    }
                }
                else if (state == CallState.Idle)
                {
                    if (_Recorder != null)
                    {
                        _Recorder.RecStop(incomingNumber);
                        _Recorder.Dispose();
                    }
                }
            }
        }
    }
}