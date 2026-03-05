

using UnityEngine;
using UnityEngine.Events;

namespace StreamEvents
{
    public class OneParameterEvent<T> : ScriptableObject
    {
        private UnityAction<T> stream;
        public void Sub(UnityAction<T> del) => stream += del;
        public void Unsub(UnityAction<T> del) => stream -= del;
        public void Invoke(T with) => stream?.Invoke(with); 
    }
}