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
            global Event playBeat;
            global Event pauseBeat;
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
            myChuck.BroadcastEvent("playBeat");
        } 
        else
        {
            myChuck.BroadcastEvent("pauseBeat");
        }
    }
}