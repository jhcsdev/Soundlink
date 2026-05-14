// this is the first beat, which will correspond to the first level

using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "LevelOne", menuName = "Sounds/LevelOne")]
    public class LevelOne : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format( @"
            // level one: kick, kick
            global Event beatStart;
            global Event beatDone;
            global float BPM;

            beatStart.signal();

            SinOsc kick => ADSR envKick => Gain kickGain => dac;

            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            2.0 => kickGain.gain;
            150 => kick.freq;
            1.0 => float KICK_GAIN;

            (60.0 / BPM)::second => dur beat_dur;

            fun void playKick(float beat_note) {{  
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envKick.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envKick.keyOn();
                hold_time => now;
                
                envKick.keyOff();
                release_time => now;
            }}

            1.0 => kick.gain;

            playKick(1.0);
            playKick(1.0);
            beatDone.signal();
            "));
        }
    }
}