
using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "KickSound", menuName = "Sounds/Kick")]
    public class KickSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format( @"
            global float BPM;

            SinOsc kick => ADSR envKick => Gain kickGain => dac;

            (.05::ms, 10::ms, 0, 10::ms) => envKick.set;
            2.0 => kickGain.gain;
            150 => kick.freq;
            1.0 => float KICK_GAIN;
            
            (60.0 / BPM)::second => dur beat_dur;

            fun void playKick(float beat_note) {{
                // turn kick on
                KICK_GAIN => kick.gain;
                
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envKick.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envKick.keyOn();
                hold_time => now;
                
                envKick.keyOff();
                release_time => now;
                
                // finally, turn off
                0 => kick.gain;
            }}

            playKick(1.0);
            "));
        }
    }
}