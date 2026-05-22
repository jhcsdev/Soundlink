using System;
using SceneTransition;
using StreamEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Menus
    {
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private BasicEventStream OpenPauseMenuStream;
        [SerializeField] private FloatEventStream PauseGameStream; // emits a 1 if paused, 0 if not
        // [SerializeField, Tooltip("This long between pausing / unpausing is mandated")] private float pauseAllowanceCooldown = 0.1f;

        public UnityAction<bool> PauseMenuStateChanged;
        public UnityAction<PauseMenuSubState> SubmenuStateChange;

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
            PauseMenuStateChanged?.Invoke(isPaused);
            SubmenuStateChange?.Invoke(isPaused ? PauseMenuSubState.MAIN_PAUSE_MENU : PauseMenuSubState.ALL_CLOSED);
            PauseGameStream.Invoke(Convert.ToInt32(isPaused));
        }

        public void HowToPlayPressed()
        {
            SubmenuStateChange?.Invoke(PauseMenuSubState.HOW_TO_PLAY);
        }
        public void ToMainPauseScreen()
        {
            SubmenuStateChange?.Invoke(PauseMenuSubState.MAIN_PAUSE_MENU);
        }

        public void BackToGamePressed()
        {
            TogglePauseMenu();
        }

        public void ToMainMenuPressed()
        {
            AsyncSceneLoader.Instance.LoadMainScene();
        }
    }

    public enum PauseMenuSubState
    {
        MAIN_PAUSE_MENU,
        HOW_TO_PLAY,
        ALL_CLOSED
    }
    }