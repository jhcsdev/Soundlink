// this is the first beat, which will correspond to the first level

using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "BeatOne", menuName = "Sounds/BeatOne")]
    public class BeatOne : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            Debug.Log($"myChuck is (BeatOne): {myChuck}");

            if (myChuck == null) Debug.Log("There is no Chuck!");

            Debug.Log("Play BeatOne!");

            myChuck.RunCode( string.Format( @"
            // super simple beat: kick, clap, kick, clap
            global Event beatStart;
            global Event beatDone;

            beatStart.signal();

            SinOsc kick => ADSR envKick => Gain kickGain => dac;
            Noise clap => BPF filter => ADSR envClap => Gain clapGain => dac;

            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            2.0 => kickGain.gain;
            150 => kick.freq;
            1.0 => float KICK_GAIN;

            (2::ms, 10::ms, 0, 5::ms) => envClap.set;
            1500 => filter.freq;
            1.5 => filter.Q;
            .85 => float CLAP_GAIN;
            2.0 => clapGain.gain;

            120 => float BPM;
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

            fun void playClap(float beat_note) {{
                // calculate hold and release times
                beat_note * beat_dur => dur total_time;
                envClap.releaseTime() => dur release_time;
                // NOTE: 50 ms is hardcoded for now [2 * (15 + 10)]
                50::ms => dur hold_time;
                total_time - release_time - hold_time => dur wait_time;
                
                for (0 => int i; i < 2; i++) {{
                    envClap.keyOn();
                    15::ms => now;
                    envClap.keyOff();
                    10::ms => now;   
                }}
                
                wait_time => now;
            }}

            1.0 => kick.gain;
            .8 => clap.gain;

            playKick(1.0);
            playClap(1.0);
            playKick(1.0);
            playClap(1.0);

            beatDone.signal();
            "));
        }
    }
}