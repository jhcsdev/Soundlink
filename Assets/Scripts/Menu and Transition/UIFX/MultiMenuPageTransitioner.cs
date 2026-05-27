using System.Collections.Generic;
using UnityEngine;

namespace UIFX
{
    public class MultiMenuPageTransitioner : MonoBehaviour, IUITransitionBaseControl<IUITransitionElement>
    {
        [SerializeField] private List<IUITransitionElement> pages;
        [SerializeField] private IUITransitionElement currentPage;
        private IUITransitionElement pendingPage;

        private bool isEntering;

        void OnEnable()
        {
            foreach (var page in pages)
            {
                if (page!= currentPage) page.Exit(BypassComplete);
            }
        }

        public void GoToPage(IUITransitionElement to) => TriggerEnter(new() { param = to });

        public void TriggerEnter(TransitionControllerParams<IUITransitionElement> focusedElement)
        {
            Debug.Log("Triggered enter in multi");
            var target = focusedElement.param;
            if (target == currentPage && pendingPage == null) return;

            pendingPage = target;

            if (currentPage != null)
            {
                if (isEntering)
                {
                    currentPage.FastKill();
                    currentPage = null;
                    isEntering = false;
                    EnterPending();
                }
                else
                {
                    currentPage.Exit(ExitComplete);
                }
            }
            else
            {
                EnterPending();
            }
        }

        public void TriggerExit(TransitionControllerParams<IUITransitionElement> _)
        {
            if (currentPage == null) return;

            if (isEntering)
            {
                currentPage.FastKill();
                currentPage = null;
                isEntering = false;
                pendingPage = null;
                return;
            }
            pendingPage = null;
            currentPage.Exit(ExitComplete);
        }

        public void EnterComplete(IUITransitionElement actor)
        {
            isEntering = false;
        }

        public void ExitComplete(IUITransitionElement actor)
        {
            currentPage = null;

            if (pendingPage != null) EnterPending();
        }

        private void EnterPending()
        {
            currentPage = pendingPage;
            pendingPage = null;
            isEntering = true;

            currentPage.Enter(EnterComplete);
        }

        private void BypassComplete(IUITransitionElement _) { /*Debug.Log("Bypass");*/ }
    }
}