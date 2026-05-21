using UnityEngine;
using UnityEngine.Events;

namespace UIFX
{
    // Monobehaviours implementing this interface should be UI elements, and their parent should have the IUITransitionBaseControl 
    // interface attached; they will subscribe to its OnEnter / OnExit actions.
    public abstract class IUITransitionElement : MonoBehaviour
    {
        public abstract void Enter(UnityAction<IUITransitionElement> onComplete);
        public abstract void Exit(UnityAction<IUITransitionElement> onComplete);
        public abstract void FastKill();
    }
}