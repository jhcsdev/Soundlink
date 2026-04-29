
using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "MetronomeSound", menuName = "Sounds/Metronome")]
    public class MetronomeSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format( @"
            global float BPM;

            SinOsc click => ADSR envClick => dac;

            (1::ms, 5::ms, 0.0, 15::ms) => envClick.set;

            880.0 => float CLICK_FREQ;
            0.6 => float CLICK_GAIN;

            (60.0 / BPM)::second => dur beat_dur;

            4 => int BEATS_PER_MEASURE;

            // Play a single metronome click
            // isAccent: true for beat 1 downbeat, false for other beats
            fun void playClick(int isAccent, float beat_note) {{
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envClick.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envClick.keyOn();
                hold_time => now;
                
                envClick.keyOff();
                release_time => now;
            }}

            playClick(0, 1.0);
            "));
        }
    }
}