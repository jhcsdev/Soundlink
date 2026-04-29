
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

            // metronome
            SinOsc click => ADSR envClick => dac;
            SinOsc clickAccent => ADSR envAccent => dac;

            (1::ms, 5::ms, 0.0, 15::ms) => envClick.set;
            (1::ms, 5::ms, 0.0, 15::ms) => envAccent.set;

            // diff freqs for click, accent (on the downbeat)
            880.0 => click.freq;
            1760.0 => clickAccent.freq; 

            0.3 => click.gain;
            0.35 => clickAccent.gain;

            // bpm and timing
            (60.0 / BPM)::second => dur beat_dur;

            // play a single metronome click
            fun void playClick(int isAccent) {{
                if (isAccent) {{
                    envAccent.releaseTime() => dur release_time;
                    beat_dur - release_time => dur hold_time;
                    
                    envAccent.keyOn();
                    hold_time => now;
                    envAccent.keyOff();
                    release_time => now;
                }} else {{
                    envClick.releaseTime() => dur release_time;
                    beat_dur - release_time => dur hold_time;
                
                    envClick.keyOn();
                    hold_time => now;
                    envClick.keyOff();
                    release_time => now;
                }}
            }}

            playClick(0);"));
        }
    }
}