using System;
using StreamEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private BasicEventStream OpenPauseMenuStream;
    [SerializeField] private FloatEventStream PauseGameStream; // emits a 1 if paused, 0 if not
    [SerializeField, Tooltip("This long between pausing / unpausing is mandated")] private float pauseAllowanceCooldown = 0.1f;

    private bool isPaused = false;

    void OnEnable()
    {
        OpenPauseMenuStream.Sub(TogglePauseMenu);
    }
    void OnDisable()
    {
        OpenPauseMenuStream.Unsub(TogglePauseMenu);
    }
    
    void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (!isPaused)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else transform.GetChild(0).gameObject.SetActive(isPaused);

        PauseGameStream.Invoke(Convert.ToInt32(isPaused));
    }

    public void HowToPlayPressed()
    {
        // todo:: currently uses the ChildContextSwapper to switch to the "How to play" menu from the pause.
    }

    public void BackToGamePressed()
    {
        TogglePauseMenu();
    }

    public void ToMainMenuPressed()
    {
        SceneManager.LoadScene(0); // todo:: should really be scene manager of its own
    }
}
