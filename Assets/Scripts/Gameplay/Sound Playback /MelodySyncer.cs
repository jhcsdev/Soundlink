using System.Collections;
using GridLinks;
using TrackSounds;
using UnityEngine;

namespace SoundPlayback
{
    public class MelodySyncer : MonoBehaviour
    {   
        [SerializeField] private TrackSound mainSoundtrack; 

        #region unity functions
        void Awake()
        {
        }

        void Start()
        {
            LinkPlaybackManager.Instance.DisableSoundPlayback();
            LinkPlaybackManager.Instance.SetPlayWinSound(mainSoundtrack, true);
        }
        #endregion
    }
}