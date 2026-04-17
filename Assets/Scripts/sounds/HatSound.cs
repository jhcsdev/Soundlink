using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "HatSound", menuName = "Sounds/Hat")]
    public class HatSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            Debug.Log("Play hat!");

            myChuck.RunCode( string.Format( @"
            Noise hat => HPF hpf => ADSR envHat => dac;

            (1::ms, 20::ms, 0, 10::ms) => envHat.set;
            8000 => hpf.freq;
            8 => hpf.Q;
            .45 => float HAT_GAIN;

            60 => float BPM;
            (60.0 / BPM)::second => dur beat_dur;

            fun void playHat(float beat_note) {{
                // turn on 
                HAT_GAIN => hat.gain;
                
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envHat.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envHat.keyOn();
                hold_time => now;
                
                envHat.keyOff();
                release_time => now;
                
                // turn off
                0 => hat.gain;
            }}

            0 => hat.gain;
            playHat(1.0);
            "));
        }
    }
}