using System;

namespace UIFX
{
    public interface IUITransitionBaseControl<T>
    {
        public void TriggerEnter(TransitionControllerParams<T> param);
        public void TriggerExit(TransitionControllerParams<T> param);
        public void EnterComplete(IUITransitionElement completedActor);
        public void ExitComplete(IUITransitionElement completedActor);
    }
    public class TransitionControllerParams<T>
    {
        public T param;
    }
}