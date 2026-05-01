using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;
using UnityEngine.EventSystems;

public class MetronomeButtonAction : MonoBehaviour
{
    public Button myButton;
    public TrackSound sound;
    private ChuckSubInstance myChuck;
    private bool isPlaying = false;
    private bool initialized = false;

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
        // deselect button so it cannot receive keyboard submit events 
        EventSystem.current.SetSelectedGameObject(null);

        isPlaying = !isPlaying;

        if (isPlaying)
        {
            LinkPlaybackManager.Instance.SetMetronomeStartTime();
            myChuck.BroadcastEvent("playMetronome");
        } 
        else
        {
            Debug.Log("Pause metronome");
            myChuck.BroadcastEvent("pauseMetronome");
        }
    }
}