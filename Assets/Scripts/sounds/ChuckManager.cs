using System.Net.Mail;
using UnityEngine;

namespace ChuckChuckChuck
{
    public class ChuckManager: MonoBehaviour
    {
        public static ChuckManager Instance;

        [SerializeField] public ChuckMainInstance chuckMainInstance;
        [SerializeField] public ChuckSubInstance chuckSubInstance;
        public static float BPM = 90f;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // define global BPM
            chuckSubInstance.RunCode( string.Format(@"
                {0} => global float BPM;
            ", BPM));
        }

        void Start()
        {
        }
    }
}