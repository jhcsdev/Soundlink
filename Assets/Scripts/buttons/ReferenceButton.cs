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
    private bool isPlaying = false;

    void Start()
    {
    }

    public void PlayReference()
    {
        // deselect button so it cannot receive keyboard submit events 
        EventSystem.current.SetSelectedGameObject(null);
        
        isPlaying = !isPlaying;

        // toggle sprite
        buttonImage.sprite = isPlaying ? pauseSprite : playSprite;

        if (isPlaying)
        {
            // instead of broadcast, use manager instance
            LinkPlaybackManager.Instance.PlayReferenceBeat();
        } 
        else
        {
            Debug.Log("Pause reference");
            LinkPlaybackManager.Instance.PauseReferenceBeat();
        }
    }
}