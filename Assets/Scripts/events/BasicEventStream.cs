

using UnityEngine;
using UnityEngine.Events;

namespace StreamEvents
{
    [CreateAssetMenu(fileName ="BasicEventStream", menuName = "StreamEvents/Basic")]
    public class BasicEventStream : ScriptableObject
    {
        private UnityAction stream;
        public void Sub(UnityAction del) => stream += del;
        public void Unsub(UnityAction del) => stream -= del;
        public void Invoke() => stream?.Invoke(); 
    }
}