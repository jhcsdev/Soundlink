using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;
using UnityEngine.EventSystems;

public class ReferenceButtonAction : MonoBehaviour
{
    [SerializeField] Sprite playSprite;
    [SerializeField] Sprite pauseSprite;
    [SerializeField] Image buttonImage;

    public Button myButton;
    public TrackSound sound;
    private ChuckSubInstance myChuck;
    private bool isPlaying = false;

    // TODO: when the reference beat starts, should make sure that everything else stops
    // and everything else should stop when i play the reference beat;
    
    // TODO: basically, the metronome, reference, and link playback have to communicate with each other .. how am I going to do that
    // FIRST: get the metronome playback working off a button or something

    // TODO: it would also be cool if everything could sync .. so you could play metronome with either the reference or the link playback and it would sync

    void Start()
    {
        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // must declare Chuck events before creating listeners
        myChuck.RunCode( string.Format( @"
            global Event playReference;
            global Event pauseReference;
        "));

        sound?.PlaySound();
    }

    public void PlayReference()
    {
        // deselect button so it cannot receive keyboard submit events 
        EventSystem.current.SetSelectedGameObject(null);
        Debug.Log("PlayReference called!\n" + System.Environment.StackTrace);

        isPlaying = !isPlaying;

        // toggle sprite
        buttonImage.sprite = isPlaying ? pauseSprite : playSprite;

        if (isPlaying)
        {
            Debug.Log("Play reference");
            myChuck.BroadcastEvent("playReference");
        } 
        else
        {
            Debug.Log("Pause reference");
            myChuck.BroadcastEvent("pauseReference");
        }
    }
}