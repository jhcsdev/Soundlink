using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;

public class ButtonAction : MonoBehaviour
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

    void Start()
    {
        myButton.onClick.AddListener(LogClick);

        // initialize text
        buttonText = myButton.GetComponentInChildren<Text>();

        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // must declare Chuck events before creating listeners
        myChuck.RunCode( string.Format( @"
            global Event beatStart;
            global Event beatDone;
        "));

        // Debug.Log($"myChuck is (RefenceButton): {myChuck}");
        // Debug.Log($"buttonText is: {buttonText}");

        // create listener for when beat starts
        // create a ChuckEventListener, call SetButtonPlaying() during Update() after every broadcast from "beatStart"
        ChuckEventListener beatStartListener = gameObject.AddComponent<ChuckEventListener>();
        beatStartListener.ListenForEvent( myChuck, "beatStart", SetButtonPlaying);

        // create listener for when beat ends
        // create a ChuckEventListener, call SetButtonIdle() during Update() after every broadcast from "beatDone"
        ChuckEventListener beatDoneListener = gameObject.AddComponent<ChuckEventListener>();
        beatDoneListener.ListenForEvent( myChuck, "beatDone", SetButtonIdle);
    }

    void LogClick()
    {
        sound?.PlaySound();
        ToggleIcon();
    }

    void ToggleIcon()
    {
        isPlaying = !isPlaying;
        buttonImage.sprite = isPlaying ? pauseSprite : playSprite;
    }

    // disable link sound playback, change button visuals
    void SetButtonPlaying()
    {
        linkManager.DisableSoundPlayback();

        ColorBlock colors = myButton.colors;
        colors.disabledColor = playingColor;
        myButton.colors = colors;
    }

    // enable link sound playback, change button visuals
    void SetButtonIdle()
    {
        linkManager.EnableSoundPlayback();

        ColorBlock colors = myButton.colors;
        colors.normalColor = idleColor;
        myButton.colors = colors;
    }
}