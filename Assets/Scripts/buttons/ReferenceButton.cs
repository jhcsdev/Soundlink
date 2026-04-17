// this is the code to do something when the reference button is hit
// for now, just a debug statement would be good .. will eventually want to reference the ChuckSound, though, huh ..

using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;

public class ButtonAction : MonoBehaviour
{
    public Button myButton;
    public TrackSound sound;

    void Start()
    {
        myButton.onClick.AddListener(LogClick);
    }

    void LogClick()
    {
        Debug.Log("Button clicked!");
        sound?.PlaySound();
    }
}