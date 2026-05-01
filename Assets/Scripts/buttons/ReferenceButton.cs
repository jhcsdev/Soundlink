using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;

public class ReferenceButtonAction : MonoBehaviour
{
    [SerializeField] Sprite playSprite;
    [SerializeField] Sprite pauseSprite;
    [SerializeField] Image buttonImage;

    public Button myButton;
    public TrackSound sound;
    public LinkPlaybackManager linkManager;
    private ChuckSubInstance myChuck;

    public Color idleColor = Color.white;
    public Color playingColor = Color.green;
    private Text buttonText;
    private bool isPlaying = false;

    // TODO: when the reference beat starts, should make sure that everything else stops
    // and everything else should stop when i play the reference beat;
    
    // TODO: basically, the metronome, reference, and link playback have to communicate with each other .. how am I going to do that
    // FIRST: get the metronome playback working off a button or something

    // TODO: it would also be cool if everything could sync .. so you could play metronome with either the reference or the link playback and it would sync

    void Start()
    {
        myButton.onClick.AddListener(LogClick);

        // initialize text
        buttonText = myButton.GetComponentInChildren<Text>();

        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // must declare Chuck events before creating listeners
        myChuck.RunCode( string.Format( @"
            global Event playReference;
            global Event pauseReference;
        "));

        sound?.PlaySound();
    }

    void LogClick()
    {
        isPlaying = !isPlaying;

        // toggle sprite
        buttonImage.sprite = isPlaying ? pauseSprite : playSprite;

        if (isPlaying)
        {
            myChuck.BroadcastEvent("playReference");
        } 
        else
        {
            myChuck.BroadcastEvent("pauseReference");
        }
    }
}