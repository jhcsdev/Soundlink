using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UIFX
{
    public class MenuPageTransitionController : IUITransitionElement, IUITransitionBaseControl<bool>
    {
        [SerializeField] private List<IUITransitionElement> initialTransitions;
        [SerializeField] private List<IUITransitionElement> sequentialTransitions;

        [Tooltip("sequential elements fire after this delay OR after the previous transition completes, whichever is sooner")]
        [SerializeField] private float sequentialDelay = 0.1f;

        private HashSet<IUITransitionElement> activeEnterTransitions;
        private HashSet<IUITransitionElement> activeExitTransitions;

        private UnityAction<IUITransitionElement> pendingEnterComplete;
        private UnityAction<IUITransitionElement> pendingExitComplete;

        private bool initialsEnterDone;
        private bool sequentialEnterDone;

        private Coroutine sequentialCoroutine;

        #region IUITransitionBase funcs
        public void TriggerEnter(TransitionControllerParams<bool> p) => Enter(null);
        public void TriggerExit(TransitionControllerParams<bool> p) => Exit(null);

        public void EnterComplete(IUITransitionElement actor)
        {
            if (activeEnterTransitions == null) return;
            activeEnterTransitions.Remove(actor);

            if (activeEnterTransitions.Count == 0)
            {
                initialsEnterDone = true;
                TryFireEnterComplete();
            }
        }

        public void ExitComplete(IUITransitionElement actor)
        {
            if (activeExitTransitions == null) return;
            activeExitTransitions.Remove(actor);

            if (activeExitTransitions.Count == 0)
            {
                var cb = pendingExitComplete;
                pendingExitComplete = null;
                cb?.Invoke(this);
            }
        }
        #endregion

        #region IUITransitionElement funcs
        public override void Enter(UnityAction<IUITransitionElement> onComplete)
        {
            // Debug.Log("ENTER: " + gameObject.name);
            pendingEnterComplete = onComplete;
            activeEnterTransitions = new HashSet<IUITransitionElement>();
            initialsEnterDone = false;
            sequentialEnterDone = false;

            if (initialTransitions != null)
            {
                foreach (var element in initialTransitions)
                {
                    activeEnterTransitions.Add(element);
                    element.Enter(EnterComplete);
                }
            }

            if (sequentialCoroutine != null) StopCoroutine(sequentialCoroutine);
            sequentialCoroutine = StartCoroutine(SequentialEnterCoroutine());

            if (activeEnterTransitions.Count == 0)
            {
                initialsEnterDone = true;
                TryFireEnterComplete();
            }
        }

        public override void Exit(UnityAction<IUITransitionElement> onComplete)
        {
            // Debug.Log("EXIT: " + gameObject.name);
            if (sequentialCoroutine != null)          // ← add this
            {                                          // ← add this
                StopCoroutine(sequentialCoroutine);    // ← add this
                sequentialCoroutine = null;            // ← add this
            }                                          // ← add this
            pendingExitComplete = onComplete;
            activeExitTransitions = new HashSet<IUITransitionElement>();

            var all = new List<IUITransitionElement>();
            if (initialTransitions != null) all.AddRange(initialTransitions);
            if (sequentialTransitions != null) all.AddRange(sequentialTransitions);

            if (all.Count == 0)
            {
                onComplete?.Invoke(this);
                return;
            }

            foreach (var element in all)
            {
                activeExitTransitions.Add(element);
                element.Exit(ExitComplete);
            }
        }

        public override void FastKill()
        {
            if (sequentialCoroutine != null)
            {
                StopCoroutine(sequentialCoroutine);
                sequentialCoroutine = null;
            }

            if (initialTransitions != null) foreach (var e in initialTransitions) e.FastKill();
            if (sequentialTransitions != null) foreach (var e in sequentialTransitions) e.FastKill();

            pendingEnterComplete = null;
            pendingExitComplete = null;

            activeEnterTransitions?.Clear();
            activeExitTransitions?.Clear();

            initialsEnterDone = false;
            sequentialEnterDone = false;
        }
        #endregion

        #region helpers
        private IEnumerator SequentialEnterCoroutine()
        {
            if (sequentialTransitions == null || sequentialTransitions.Count == 0)
            {
                sequentialEnterDone = true;
                TryFireEnterComplete();
                yield break;
            }

            foreach (var element in sequentialTransitions)
            {
                bool elementComplete = false;
                element.Enter(_ => elementComplete = true);

                // Wait for the delay OR for the element to finish, whichever is sooner.
                float elapsed = 0f;
                while (elapsed < sequentialDelay && !elementComplete)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            sequentialCoroutine = null;
            sequentialEnterDone = true;
            TryFireEnterComplete();
        }

        private void TryFireEnterComplete()
        {
            if (!initialsEnterDone || !sequentialEnterDone) return;

            var cb = pendingEnterComplete;
            pendingEnterComplete = null;
            cb?.Invoke(this);
        }
        #endregion
    }
}