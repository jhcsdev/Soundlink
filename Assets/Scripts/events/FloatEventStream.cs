

using UnityEngine;

namespace StreamEvents
{
    [CreateAssetMenu(fileName = "FloatEventStream", menuName = "StreamEvents/Float Event")]
    public class FloatEventStream : OneParameterEvent<float>
    { }
}