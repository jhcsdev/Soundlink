using UnityEngine;
using UnityEngine.UI;
using ChuckChuckChuck;
using TrackSounds;
using GridLinks;
using UnityEngine.EventSystems;

public class MetronomeButtonAction : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public TrackSound sound;
    public RectTransform pendulum;      
    public Image bodyImage;
    public Image pendulumImage;
    [Header("Settings")]
    public float swingAngle = 30f;

    private ChuckSubInstance myChuck;
    private bool isPlaying = false;
    // TODO: make the BPM global? i guess Chuck could broadcast?
    private float bpm = 90f;
    private float swingTimer = 0f;

    void Start()
    {
        myChuck = ChuckManager.Instance.chuckSubInstance;
        sound?.PlaySound();
        if (bodyImage) bodyImage.color = new Color(0.8f, 0.8f, 0.8f);
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
        EventSystem.current.SetSelectedGameObject(null);
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            swingTimer = 0f;
            LinkPlaybackManager.Instance.SetMetronomeStartTime();
            LinkPlaybackManager.Instance.SetMetronnomePlaying();
        }
        else
        {
            LinkPlaybackManager.Instance.SetMetronnomePaused();
            pendulum.localRotation = Quaternion.Euler(0, 0, 0); // reset to center
        }
    }
}