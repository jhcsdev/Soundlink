using UnityEngine;
using UnityEngine.UI;
using GridLinks;
using UnityEngine.EventSystems;

public class ReferenceButtonAction : MonoBehaviour
{
    [SerializeField] Sprite playSprite;
    [SerializeField] Sprite pauseSprite;
    [SerializeField] Image buttonImage;
    [SerializeField] MetronomeButtonAction metronomeButton;
    private bool isPlaying = false;

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
            metronomeButton.SetInteractable(false);
        } 
        else
        {
            LinkPlaybackManager.Instance.PauseReferenceBeat();
            metronomeButton.SetInteractable(true);
        }
    }
}