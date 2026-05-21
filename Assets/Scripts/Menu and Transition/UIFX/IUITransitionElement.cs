using UnityEngine.Events;

namespace UIFX
{
    // Monobehaviours implementing this interface should be UI elements, and their parent should have the IUITransitionBaseControl 
    // interface attached; they will subscribe to its OnEnter / OnExit actions.
    public interface IUITransitionElement
    {
        public void Enter(UnityAction<IUITransitionElement> onComplete);
        public void Exit(UnityAction<IUITransitionElement> onComplete);
        public void FastKill();
    }
}