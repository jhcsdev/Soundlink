using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;

public class MetronomeButtonAction : MonoBehaviour
{
    public Button myButton;
    public TrackSound sound;
    private ChuckSubInstance myChuck;
    private bool isPlaying = false;
    private bool initialized = false;

    // TODO: play the metronome sound when on click, stop when not cliced

    void Start()
    {
        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // must declare Chuck events before creating listeners
        myChuck.RunCode( string.Format( @"
            global Event playMetronome;
            global Event pauseMetronome;
        "));

        sound?.PlaySound();
    }


    public void PlayMetronome()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            Debug.Log("Play metronome");
            myChuck.BroadcastEvent("playMetronome");
        } 
        else
        {
            Debug.Log("Pause metronome");
            myChuck.BroadcastEvent("pauseMetronome");
        }
    }
}