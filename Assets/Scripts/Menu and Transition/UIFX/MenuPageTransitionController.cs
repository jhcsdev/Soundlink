using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UIFX
{
    public class MenuPageTransitionController : MonoBehaviour, IUITransitionBaseControl<bool>, IUITransitionElement
    {
        [SerializeField] private List<IUITransitionElement> initialTransitions;
        [SerializeField] private List<IUITransitionElement> sequentialTransitions;

        private HashSet<IUITransitionElement> activeEnterTransitions;
        private HashSet<IUITransitionElement> activeExitTransitions;

        public void TriggerEnter(TransitionControllerParams<bool> p)
        {
            
        }
        public void TriggerExit(TransitionControllerParams<bool> p)
        {
            
        }
        public void EnterComplete(IUITransitionElement actor)
        {
            
        }
        public void ExitComplete(IUITransitionElement actor)
        {
            
        }
        public void Enter(UnityAction<IUITransitionElement> onComplete)
        {
            
        }
        public void Exit(UnityAction<IUITransitionElement> onComplete)
        {
            
        }
    }
}