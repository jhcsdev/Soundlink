// this is the first beat, which will correspond to the first level

using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "LevelTwo", menuName = "Sounds/LevelTwo")]
    public class LevelTwo : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            Debug.Log("Level two reference!");

            myChuck.RunCode( string.Format( @"
            // level two: pause, kick
            global Event playReference;
            global Event pauseReference;
            global float BPM;

            SinOsc kick => ADSR envKick => dac;

            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            150 => kick.freq;
            1.2 => kick.gain;

            (60.0 / BPM)::second => dur beat_dur;
            beat_dur / 4.0 => dur sixteenth;

            fun void playKick(float beat_note) {{  
                // calculate hold and release times
                beat_note * sixteenth => dur total_time;
                envKick.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envKick.keyOn();
                hold_time => now;
                
                envKick.keyOff();
                release_time => now;
            }}

            fun void Rest(float beat_note) {{
                0 => kick.gain;
                beat_note * sixteenth => now;
                1.0 => kick.gain;
            }}

            fun void kickPattern() {{
                playKick(4.0);  
                Rest(4.0);
            }}

            fun void beatLoop() {{
                beat_dur - (now % beat_dur) => now;  // snap to grid
                while (true) {{
                    spork ~ kickPattern();
                    8.0 * sixteenth => now;  
                }}
            }}

            while (true) {{
                playReference => now;
                spork ~ beatLoop() @=> Shred @ myShred;
                pauseReference => now;
                myShred.exit();
            }}
            "));
        }
    }
}