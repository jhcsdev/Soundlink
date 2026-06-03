
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

            // -----------
            // play the loop 
            // -----------

            spork ~ Synth();
            spork ~ Arp();

            while (true) {{
                16::beat => now;
            }}
            "));
        }
    }
}