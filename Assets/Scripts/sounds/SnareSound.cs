
using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "SnareSound", menuName = "Sounds/Snare")]
    public class SnareSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            Debug.Log("Play snare!");

            myChuck.RunCode( string.Format( @"
            global float BPM;

            Noise snare => LPF lpf => ADSR envSnare => dac;

            (2::ms, 50::ms, 0, 10::ms) => envSnare.set;
            1800 => lpf.freq;
            1.5 => lpf.Q;
            .7 => float SNARE_GAIN;

            (60.0 / BPM)::second => dur beat_dur;

            // play snare sound for given beat duration
            fun void playSnare(float beat_note) {{
                // turn kick on
                SNARE_GAIN => snare.gain;
                
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envSnare.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envSnare.keyOn();
                hold_time => now;
                
                envSnare.keyOff();
                release_time => now;
                
                // finally, turn off
                0 => snare.gain;
            }}

            playSnare(1.0);
            "));
        }
    }
}