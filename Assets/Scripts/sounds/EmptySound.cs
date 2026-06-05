using ChuckChuckChuck;
using UnityEngine;

namespace TrackSounds
{
    [CreateAssetMenu(fileName = "EmptySound", menuName = "Sounds/EmptySound")]
    public class EmptySound : TrackSound
    {
        private static ChuckSubInstance myChuck;

        public override void PlaySound()
        {
            // plays nothing. this is used for main menu
        }
    }
}