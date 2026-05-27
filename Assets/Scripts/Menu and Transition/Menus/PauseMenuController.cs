using System;
using SceneTransition;
using StreamEvents;
using UIFX;
using UnityEngine;

namespace Menus
{
    [RequireComponent(typeof(MultiMenuPageTransitioner))]
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private BasicEventStream OpenPauseMenuStream;
        [SerializeField] private FloatEventStream PauseGameStream; // emits a 1 if paused, 0 if not
        // [SerializeField, Tooltip("This long between pausing / unpausing is mandated")] private float pauseAllowanceCooldown = 0.1f;
        [SerializeField] private IUITransitionElement pauseClosed;
        [SerializeField] private IUITransitionElement pauseOpened;

        private MultiMenuPageTransitioner transitioner;

        private bool isPaused = false;

        void Awake()
        {
            transitioner = GetComponent<MultiMenuPageTransitioner>();
        }

        void OnEnable()
        {
            OpenPauseMenuStream.Sub(TogglePauseMenu);
        }
        void OnDisable()
        {
            OpenPauseMenuStream.Unsub(TogglePauseMenu);
        }
        
        public void TogglePauseMenu()
        {
            isPaused = !isPaused;
            transitioner.GoToPage(isPaused ? pauseOpened : pauseClosed);
            PauseGameStream.Invoke(Convert.ToInt32(isPaused));
        }

        public void ChangeMenu(IUITransitionElement menu)
        {
            pauseOpened = menu;
            transitioner.GoToPage(isPaused ? pauseOpened : pauseClosed);
        }

        public void ToMainMenuPressed()
        {
            AsyncSceneLoader.Instance.LoadMainScene();
        }
    }
}