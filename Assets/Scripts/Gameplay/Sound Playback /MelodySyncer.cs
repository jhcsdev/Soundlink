using System.Collections;
using GridLinks;
using TrackSounds;
using UnityEngine;

namespace SoundPlayback
{
    public class MelodySyncer : MonoBehaviour
    {   
        [SerializeField] private TrackSound introductionTrackSound;
        [SerializeField, Header("Will be (roughly) synced to grid")] private TrackSound mainSoundtrack; 
        [SerializeField] private int introductionBeatLength;
        [SerializeField] private float bpm;

        private float secondsPerBeat;

        #region unity functions
        void Awake()
        {
            secondsPerBeat = 60f / bpm / 4f;
        }

        void Start()
        {
            LinkPlaybackManager.Instance.DisableSoundPlayback();
            StartCoroutine(PlaybackLoop());
        }
        #endregion

        private IEnumerator PlaybackLoop()
        {
            WaitForSeconds beatWaitTime = new(secondsPerBeat);
            
            introductionTrackSound.PlaySound();
            for(int i = 0; i < introductionBeatLength; i++)
            {
                yield return beatWaitTime;
            }
               
            LinkPlaybackManager.Instance.ResetPlayback();
            LinkPlaybackManager.Instance.SetPlayWinSound(mainSoundtrack, true);
        }
    }
}