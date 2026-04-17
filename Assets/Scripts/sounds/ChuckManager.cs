using System.Net.Mail;
using UnityEngine;

namespace ChuckChuckChuck
{
    public class ChuckManager: MonoBehaviour
    {
        public static ChuckManager Instance;

        [SerializeField] public ChuckMainInstance chuckMainInstance;
        [SerializeField] public ChuckSubInstance chuckSubInstance;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // TODO: here ... define functions that are going to be shared across the sounds and stuff like that.
        }

        void Start()
        {
            // TODO: add if I want ... 
        }
    }
}