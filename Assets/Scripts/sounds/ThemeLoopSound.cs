
using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "ThemeLoopSound", menuName = "Sounds/ThemeLoop")]
    public class ThemeLoopSound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            myChuck = ChuckManager.Instance.chuckSubInstance;

            if (myChuck == null) Debug.Log("There is no Chuck!");

            myChuck.RunCode( string.Format( @"
            /* create channels */
            TriOsc synth1 => ADSR envSynth => dac;
            TriOsc synth2 => envSynth;
            TriOsc synth3 => envSynth;
            TriOsc arp => ADSR envArp => dac;
            SinOsc kick => ADSR envKick => dac;
            Noise clap => BPF filter => ADSR envClap => dac;
            Noise hat => HPF hpfHat => ADSR envHat => dac;

            // shape sounds
            (2::ms, 10::ms, 0, 10::ms) => envKick.set;
            150 => kick.freq;

            (2::ms, 10::ms, 0, 5::ms) => envClap.set;
            1500 => filter.freq;
            1.5 => filter.Q;
            .45 => float CLAP_GAIN;
            CLAP_GAIN => clap.gain;

            (1::ms, 20::ms, 0, 10::ms) => envHat.set;
            8000 => hpfHat.freq;
            8 => hpfHat.Q;
            .025 => float HAT_GAIN;
            HAT_GAIN => hat.gain;


            /* mix */
            .1 => synth1.gain;
            .1 => synth2.gain;
            .1 => synth3.gain;
            .15 => arp.gain;

            // define tempo: 60 bpm 
            60 => float BPM;
            (60.0 / BPM)::second => dur beat;

            /* define envs */
            (.125::beat, .125::beat, .15, 1::ms) => envSynth.set;
            (.125::beat, .125::beat, .1, 1::ms) => envArp.set;

            /* chords */
            [0, 4, 7] @=> int cMajor[];
            [2, 5, 9, 14] @=> int dMinor[];
            [4, 7, 11] @=> int eMinor[];
            [4, 8, 11] @=> int eMajor[];

            /* given chord, position: play all three notes of chord */
            fun void playNote(int chord[], int position) {{
                60 => int offset;
                // convert 'key number' to frequency
                Std.mtof(chord[0] + offset + position) => synth1.freq;
                Std.mtof(chord[1] + offset + position) => synth2.freq;
                Std.mtof(chord[2] + offset + position) => synth3.freq;
                1 => envSynth.keyOn;
                1::beat => now;
            }}

            fun void Synth() {{
                while (true) {{
                    playNote(cMajor, 0);
                    playNote(dMinor, -2);
                    playNote(cMajor, -2);
                    playNote(dMinor, -4);
                }}  
            }}

            fun void Arp() {{
                48 => int offset;
                0 => int position;
                
                /* play music */
                while (true) {{
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[1] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[2] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now;

                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[1] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[2] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now;
                    
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[1] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[2] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now;

                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[1] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[0] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now; 
                
                    Std.mtof(dMinor[3] + offset + position) => arp.freq;
                    1 => envArp.keyOn;
                    .25::beat => now;
                }}
            }}

            fun void playKick(float beat_note) {{    
                // calculate hold and release times
                beat_note * beat => dur total_time;
                envKick.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                envKick.keyOn();
                hold_time => now;
                
                envKick.keyOff();
                release_time => now;
            }}

            fun void restClap(float beat_note) {{
                beat_note * beat => dur total_time;
                0 => clap.gain;
                total_time => now;
                CLAP_GAIN => clap.gain;   
            }}

            fun void playHat(float beat_note, float velocity) {{    
                // calculate hold and release times
                beat_note * beat => dur total_time;
                envHat.releaseTime() => dur release_time;
                total_time - release_time => dur hold_time;
                
                // play sound
                velocity => hat.gain;
                envHat.keyOn();
                hold_time => now;
                
                envHat.keyOff();
                release_time => now;
            }}

            fun void kickPattern() {{
                while (true) {{
                    playKick(.75);
                    playKick(1.25);
                    playKick(.75);
                    playKick(.25);
                    playKick(.375);
                    playKick(.375);
                    playKick(.25);
                }}
            }}

            fun void clapPattern() {{
                while (true) {{
                    // rest for 1 beat
                    1::beat => now; 
                    
                    // double hit
                    envClap.keyOn();
                    15::ms => now;
                    envClap.keyOff();
                    10::ms => now;
                    envClap.keyOn();
                    15::ms => now;
                    envClap.keyOff();
                    
                    // fill remaining time to hit beat 4 (3 beats - 40ms used)
                    3::beat - 40::ms => now;
                }}
            }}

            fun void hatPattern() {{
                while (true) {{
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.125, HAT_GAIN);
                    playHat(.125, HAT_GAIN);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.125, HAT_GAIN);
                    playHat(.125, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                    playHat(.25, HAT_GAIN);
                    playHat(.25, HAT_GAIN/2);
                }}   
            }}

            // -----------
            // play the beat (without drums)
            // -----------

            spork ~ Synth();
            8::beat => now;
            spork ~ Arp();
            spork ~ kickPattern();
            spork ~ clapPattern();
            spork ~ hatPattern();

            while (true) {{
                16::beat => now;
            }}
            "));
        }
    }
}