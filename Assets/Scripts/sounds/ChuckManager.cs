using System.Net.Mail;
using UnityEngine;

namespace ChuckChuckChuck
{
    public class ChuckManager: MonoBehaviour
    {
        public static ChuckManager Instance;

        [SerializeField] public ChuckMainInstance mainInstance;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // TODO: add if I want ... 
        }
    }
}