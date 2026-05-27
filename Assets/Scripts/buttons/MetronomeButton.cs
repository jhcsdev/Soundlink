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

    void Start()
    {
        // grab same chuck subsinstance as track sound 
        myChuck = ChuckManager.Instance.chuckSubInstance;

        // initialize sound
        sound?.PlaySound();
    }

    public void SetInteractable(bool interactable)
    {
        myButton.interactable = interactable;
    }

    public void PlayMetronome()
    {
        // deselect button so it cannot receive keyboard submit events 
        EventSystem.current.SetSelectedGameObject(null);

        isPlaying = !isPlaying;

        if (isPlaying)
        {
            LinkPlaybackManager.Instance.SetMetronomeStartTime();
            LinkPlaybackManager.Instance.SetMetronnomePlaying();
        } 
        else
        {
            LinkPlaybackManager.Instance.SetMetronnomePaused();
        }
    }
}