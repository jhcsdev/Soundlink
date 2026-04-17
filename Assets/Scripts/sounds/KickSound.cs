
using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "KickSound", menuName = "Sounds/Kick")]
    public class KickSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        // TODO: there should be some way to define the function globally, no? 

        public override void PlaySound()
        {
<<<<<<< HEAD
            if (myChuck == null)
            {
                Debug.Log("Creating Object");
                GameObject chuckMain = GameObject.Find("TheChuck");
                myChuck = chuckMain.AddComponent<ChuckSubInstance>();
                myChuck.chuckMainInstance = ChuckManager.Instance.mainInstance;
            }

            Debug.Log("Play Kick!");
            myChuck.RunCode( string.Format( @"
            Noise clap => BPF filter => ADSR envClap => Gain clapGain => dac;

            (2::ms, 10::ms, 0, 5::ms) => envClap.set;
            1500 => filter.freq;
            1.5 => filter.Q;
            .85 => float CLAP_GAIN;
            2.0 => clapGain.gain;

            60 => float BPM;
            (60.0 / BPM)::second => dur beat_dur;

            fun void playClap(float beat_note) {{
                CLAP_GAIN => clap.gain;
                
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
                
                0 => clap.gain;
            }}

            playClap(1.0);
            "));
=======

            throw new System.NotImplementedException();
>>>>>>> main
        }
    }
}