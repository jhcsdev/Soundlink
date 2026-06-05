using System.Net.Mail;
using UnityEngine;
using GridLinks;

namespace ChuckChuckChuck
{
    public class ChuckManager: MonoBehaviour
    {
        public static ChuckManager Instance;

        [SerializeField] public ChuckMainInstance chuckMainInstance;
        [SerializeField] public ChuckSubInstance chuckSubInstance;
        // TODO: this means that BPM is defined here and not where we think it is ... how can we expose this? 
        // public static float BPM = 90f;

        void Awake()
        {
            if (Instance == null) { 
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        void Start()
        {
            float bpm = LinkPlaybackManager.Instance.getBPM();

            // define global BPM
            chuckSubInstance.RunCode( string.Format(@"
                {0} => global float BPM;
            ", bpm));
        }
    }
}