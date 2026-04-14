
using UnityEngine;

namespace TrackSounds
{
    public abstract class TrackSound : ScriptableObject
    {
        // Implement in lower-level sounds
        public abstract void PlaySound();
    }
}