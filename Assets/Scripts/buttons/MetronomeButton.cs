using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;
using UnityEngine.EventSystems;

public class MetronomeButtonAction : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] Button referenceButton;
    public TrackSound sound;
    public RectTransform pendulum;      
    public Image bodyImage;
    public Image centerLineImage;
    public Image pendulumImage;
    [Header("Settings")]
    public float swingAngle = 30f;

    private ChuckSubInstance myChuck;
    private bool isPlaying = false;
    // TODO: make the BPM global? i guess Chuck could broadcast?
    private float bpm = 90f;
    private float swingTimer = 0f;
    public bool isInteractable = true;

    Color yesInteractColor = new Color(0.8f, 0.8f, 0.8f);
    Color noInteractColor = new Color(0.57f, 0.57f, 0.57f);

    void Start()
    {
        myChuck = ChuckManager.Instance.chuckSubInstance;
        sound?.PlaySound();
        if (bodyImage) bodyImage.color = yesInteractColor;
        // if (outlineImage) outlineImage.color = Color.white;
        if (pendulumImage) pendulumImage.color = Color.white;
        if (centerLineImage) centerLineImage.color = Color.white;
    }

    void Update()
    {
        if (!isPlaying || pendulum == null) return;

        // one full swing (left -> right -> left) = 2 beats
        float beatsPerSecond = bpm / 60f;
        float swingSpeed = beatsPerSecond * Mathf.PI; // radians per second

        swingTimer += Time.deltaTime * swingSpeed;
        float angle = Mathf.Sin(swingTimer) * swingAngle;
        pendulum.localRotation = Quaternion.Euler(0, 0, -angle);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // block clicks when locked
        if (!isInteractable) return; 

        EventSystem.current.SetSelectedGameObject(null);
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            referenceButton.interactable = false;
            swingTimer = 0f;
            LinkPlaybackManager.Instance.SetMetronomeStartTime();
            LinkPlaybackManager.Instance.SetMetronnomePlaying();
        }
        else
        {
            referenceButton.interactable = true;;
            LinkPlaybackManager.Instance.SetMetronnomePaused();
            pendulum.localRotation = Quaternion.Euler(0, 0, 0); // reset to center
        }
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        // change color 
        if (bodyImage) 
        bodyImage.color = interactable ? yesInteractColor : noInteractColor;

        // if (outlineImage)
        // outlineImage.color = interactable ? Color.white : noInteractColor;
        
        if (pendulumImage)
        pendulumImage.color = interactable ? Color.white : noInteractColor;

        if (centerLineImage)
        centerLineImage.color = interactable ? Color.white : noInteractColor;
    }

}