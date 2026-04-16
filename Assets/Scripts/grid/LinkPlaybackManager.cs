
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace GridLinks
{
    public class LinkPlaybackManager : MonoBehaviour
    {
        public static LinkPlaybackManager Instance;

        [SerializeField] float bpm;
        [SerializeField, Tooltip("number of beats for the playback loop")] private int beatsInLoop;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            StartCoroutine(PlaybackLoop());
        }

        private IEnumerator PlaybackLoop()
        {
            yield break;
        }

        private void ParseLinks() // turns the list of grid-links into a list of "parsable" objects that are easier to play sound with 
        {
            
        }

        private void ParseSingleLink()
        {
            
        }

        public void ScheduledUpdatePlayback()
        {
            
        }

        public void ImmediateUpdateLinkPlayback()
        {
            
        }
    }
}