using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SceneTransition
{
    public class AsyncSceneLoader : MonoBehaviour
    {
        public static AsyncSceneLoader Instance;
        private bool allowStartLoading = false;

        [SerializeField] private Image transitionScreenBackdrop;
        [SerializeField] private Color backgroundFadeColor = new(0, 0, 0, 1);
        [SerializeField] private float fadeToBlackTime = 0.5f;
        [SerializeField] private float fadeFromBlackTime = 0.5f;
        [SerializeField] private float holdBlackTime = 1;
        private Color invisibleColor = new(0, 0, 0, 0);

        #region animation sequences
        private Sequence fadeFromBlackSequence;
        private Sequence fadeToBlackSequence;
        #endregion

        public void LoadMainScene()
        {
            StartCoroutine(AsyncLoad("MainScene"));
        }
        public void LoadLevelScene()
        {
            StartCoroutine(AsyncLoad("LevelScene"));
        }

        void Awake()
        {
            if (Instance == null) { 
                Instance = this; 
                // DontDestroyOnLoad(this); 
                DontDestroyOnLoad(transitionScreenBackdrop.transform.root.gameObject); // the root of the transition canvas
            }
            else Destroy(transform.root.gameObject);
        }

        void OnEnable()
        {
            SceneManager.activeSceneChanged += FadeFromBlackWrapper;
        }

        void Start()
        {
            FadeFromBlack();
        }

        private void AllowFinishLoading() { allowStartLoading = true; }
        public IEnumerator AsyncLoad(string loadScene)
        {
            FadeToBlack();

            WaitUntil sceneWait = new(() => allowStartLoading);
            yield return sceneWait;
            allowStartLoading = false; 

            AsyncOperation op = SceneManager.LoadSceneAsync(loadScene);
            op.allowSceneActivation = false;

            while (op.progress < 0.89) yield return null;

            op.allowSceneActivation = true;
        }

        public IEnumerator AsyncLoad(int sceneIndex)
        {
            FadeToBlack();

            WaitUntil sceneWait = new(() => allowStartLoading);
            yield return sceneWait;
            allowStartLoading = false; 

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
            op.allowSceneActivation = false;

            while (op.progress < 0.89) yield return null; // just wait until it's finished now, if we've already passed minimum time

            op.allowSceneActivation = true;
        }

        void FadeToBlack()
        {
            if (fadeToBlackSequence == null)
            {
                fadeToBlackSequence = DOTween.Sequence()
                    .AppendCallback(
                        () => transitionScreenBackdrop.gameObject.SetActive(true)
                    )
                    .Append(
                        transitionScreenBackdrop.DOColor(backgroundFadeColor, fadeToBlackTime).ChangeStartValue(invisibleColor)
                    ).AppendCallback(
                        () => allowStartLoading = true
                    ).SetAutoKill(false); 
            }
            if (fadeFromBlackSequence != null && fadeFromBlackSequence.active) fadeFromBlackSequence.Complete();
            fadeToBlackSequence.Restart();

        }
        void FadeFromBlackWrapper(Scene _, Scene __) 
        {
            FadeFromBlack();
        }
        void FadeFromBlack()
        {
            StartCoroutine(RunFadeFromBlack());
        }
        private IEnumerator RunFadeFromBlack()
        {
            yield return new WaitForSeconds(holdBlackTime);
            
            if (fadeFromBlackSequence == null)
            {
                fadeFromBlackSequence = DOTween.Sequence()
                    .Append(
                        transitionScreenBackdrop.DOColor(invisibleColor, fadeFromBlackTime).ChangeStartValue(backgroundFadeColor)
                    ).AppendCallback(
                        () => transitionScreenBackdrop.gameObject.SetActive(false)
                    ).SetAutoKill(false); 
            }
            if (fadeToBlackSequence != null && fadeToBlackSequence.active) fadeToBlackSequence.Complete();
            fadeFromBlackSequence.Restart();
        }
    }
}