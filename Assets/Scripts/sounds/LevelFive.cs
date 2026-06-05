// this is the first beat, which will correspond to the first level

using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "JamesBeat", menuName = "Sounds/JamesBeat")]
    public class JamesBeat : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format(@"
            // super simple beat: kick, clap, kick, clap
            global Event playReference;
            global Event pauseReference;
            global float BPM;

            // load sounds
            SinOsc kick => ADSR envKick => Gain kickGain => dac;
            Noise clap => BPF filter => ADSR envClap => Gain clapGain => dac;
            Noise hat => HPF hpf => ADSR envHat => dac;

            // shape sounds
            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            2.0 => kickGain.gain;
            150 => kick.freq;
            1.0 => float KICK_GAIN;

            (2::ms, 10::ms, 0, 5::ms) => envClap.set;
            1500 => filter.freq;
            1.5 => filter.Q;
            .85 => float CLAP_GAIN;
            2.0 => clapGain.gain;

            (1::ms, 20::ms, 0, 10::ms) => envHat.set;
            8000 => hpf.freq;
            8 => hpf.Q;
            .20 => float HAT_GAIN;

            // set global BPM
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
                // turn on 
                HAT_GAIN => hat.gain;
                
                // calculate hold and release times
                beat_note * sixteenth => dur total_time;
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

            fun void kickPattern() {{
                playKick(3.0);
                playKick(3.0);
                playKick(2.0);
            }}

            // rest for first four bars,
            // then play
            // TODO: should create function but getting working for now
            fun void clapPattern() {{
                0 => clap.gain;
                4.0 * sixteenth => dur rest_time;
                rest_time => now;
                CLAP_GAIN => clap.gain;
                playClap(4.0);
            }}

            fun void hatPattern() {{
                playHat(2.0);
                playHat(2.0);
                playHat(2.0);
                playHat(1.0);
                playHat(1.0);
            }}

            // TODO: going to want to play this on repeat when the event is true, i think ...

            // play whole beat once
            fun void wholePattern() {{
                spork ~ kickPattern();
                spork ~ clapPattern();
                spork ~ hatPattern();
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