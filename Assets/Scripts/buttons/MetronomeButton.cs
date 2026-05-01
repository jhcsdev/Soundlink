using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;

public class MetronomeButtonAction : MonoBehaviour
{
    public Button myButton;
    public TrackSound sound;
    public LinkPlaybackManager linkManager;
    private ChuckSubInstance myChuck;
    private bool isPlaying = false;

    // TODO: play the metronome sound when on click, stop when not cliced

    void Start()
    {
        myButton.onClick.AddListener(LogClick);
        
        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // must declare Chuck events before creating listeners
        myChuck.RunCode( string.Format( @"
            global Event playMetronome;
            global Event pauseMetronome;
        "));

        sound?.PlaySound();
    }

    void LogClick()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            myChuck.BroadcastEvent("playMetronome");
        } 
        else
        {
            myChuck.BroadcastEvent("pauseMetronome");
        }
    }
}