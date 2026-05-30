// this is the first beat, which will correspond to the first level

using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "LevelSix", menuName = "Sounds/LevelSix")]
    public class LevelSix : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format(@"
            // level six: all four sounds
            global Event playReference;
            global Event pauseReference;
            global float BPM;

            // load sounds
            SinOsc kick => ADSR envKick => dac;
            Noise clap => BPF filter => ADSR envClap => dac;
            Noise hat => HPF hpf => ADSR envHat => dac;
            Noise snare => LPF lpf => ADSR envSnare => dac;

            // shape sounds
            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            150 => kick.freq;
            1.0 => kick.gain;

            (2::ms, 10::ms, 0, 5::ms) => envClap.set;
            1500 => filter.freq;
            1.5 => filter.Q;
            .85 => clap.gain;

            (1::ms, 20::ms, 0, 10::ms) => envHat.set;
            8000 => hpf.freq;
            8 => hpf.Q;
            .20 => float HAT_GAIN;
            HAT_GAIN => hat.gain;

            (2::ms, 50::ms, 0, 10::ms) => envSnare.set;
            1800 => lpf.freq;
            1.5 => lpf.Q;
            .7 => snare.gain;

            // set BPM
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

            fun void playClap(float beat_note) {{
                // calculate hold and release times
                beat_note * sixteenth => dur total_time;
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

            fun void playHat(float beat_note) {{  
                // calculate hold and release times
                beat_note * sixteenth => dur total_time;
                envHat.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envHat.keyOn();
                hold_time => now;
                
                envHat.keyOff();
                release_time => now;
            }}

            // play snare sound for given beat duration
            fun void playSnare(float beat_note) {{
                // calculate hold and release times
                beat_note * sixteenth => dur total_time;
                envSnare.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envSnare.keyOn();
                hold_time => now;
                
                envSnare.keyOff();
                release_time => now;
            }}

            fun void restKick(float beat_note) {{
                0 => kick.gain;
                beat_note * sixteenth => now;
                1.0 => kick.gain;   
            }}

            fun void restClap(float beat_note) {{
                0 => clap.gain;
                beat_note * sixteenth => now;
                .85 => clap.gain;   
            }}

            fun void restHat(float beat_note) {{
                0 => hat.gain;
                beat_note * sixteenth => now;
                .20 => hat.gain;   
            }}

            fun void restSnare(float beat_note) {{
                0 => snare.gain;
                beat_note * sixteenth => now;
                .70 => snare.gain;   
            }}

            fun void kickPattern() {{
                playKick(3.0);
                playKick(1.0);
                playKick(2.0);
                playKick(2.0);
            }}

            fun void clapPattern() {{
                restClap(5.0);
                playClap(3.0);
            }}

            fun void hatPattern() {{
                playHat(2.0);
                playHat(2.0);
                playHat(1.0);
                playHat(2.0);
                playHat(1.0);
            }}

            fun void snarePattern() {{
                restSnare(4.0);
                playSnare(4.0);
            }}

            // play whole beat once
            fun void wholePattern() {{
                spork ~ kickPattern();
                spork ~ clapPattern();
                spork ~ hatPattern();
                spork ~ snarePattern();
                8.0 * sixteenth => now;    
            }}

            // loop the whole beat
            fun void beatLoop() {{
                beat_dur - (now % beat_dur) => now;  // snap to grid
                while (true) {{
                    spork ~ wholePattern();
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