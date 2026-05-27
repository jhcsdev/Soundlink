using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamePieces;
using PuzzleGrid;
using UnityEngine;
using TrackSounds;
using ChuckChuckChuck;
using SceneTransition;
using UnityEditor.XR;
using Unity.VisualScripting;

namespace GridLinks
{
    [RequireComponent(typeof(PuzzleGrid.PuzzleGrid))]
    public class LinkPlaybackManager : MonoBehaviour
    {
        public static LinkPlaybackManager Instance;

        [SerializeField] float bpm;
        [SerializeField, Tooltip("number of beats for the playback loop")] private int beatsInLoop = -1;
        [SerializeField] TrackSound referenceSound;
        private float secondsPerBeat;
        private PuzzleGrid.PuzzleGrid puzzleGrid;

        [SerializeField] private bool soundPlaybackEnabled;

        private Dictionary<int /*soundid*/, SchedulerInformation> knownLinks = new();
        private class SchedulerInformation { public List<BeatData> scheduledBeats; public GridLink link; }
        private class BeatData { public bool firstInPiece; public bool silent; public int indexInLink; }
        private ChuckSubInstance myChuck;
        private int curBeat = 1;
        private float metronomeStartTime = -1f;
        [SerializeField] bool isMetronomePlaying = false;
        public MetronomeButtonAction metronomeButtonAction;

        #region unity functions
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            puzzleGrid = GetComponent<PuzzleGrid.PuzzleGrid>();
            secondsPerBeat = 60f / bpm / 4f;
        }

        // stop link playback and play reference beat
        public void PlayReferenceBeat()
        {
            soundPlaybackEnabled = false;
            myChuck.BroadcastEvent("playReference");
            metronomeButtonAction.SetInteractable(false);
        }

        // stop reference beat and start link playback
        public void PauseReferenceBeat()
        {
            myChuck.BroadcastEvent("pauseReference");
            metronomeButtonAction.SetInteractable(true);

            // pause linkplayback for 1.5 seconds to ensure no overlap
            StartCoroutine(pauseLinkPlaybackForSeconds(1.5f));
        }

        private IEnumerator pauseLinkPlaybackForSeconds(float numSeconds)
        {
            soundPlaybackEnabled = false;
            yield return new WaitForSeconds(numSeconds); 
            soundPlaybackEnabled = true;
        }

        public void SetMetronomeStartTime()
        {
            metronomeStartTime = Time.time;
        }

        public IEnumerator SyncAndStart()
        {
            if (metronomeStartTime < 0f)
            {
                // metronome never started, just begin immediately
                curBeat = 1;
                EnableSoundPlayback();
                yield break;
            }

            float T = secondsPerBeat * beatsInLoop;
            float elapsed = Time.time - metronomeStartTime;
            float timeUntilNextMeasure = T - (elapsed % T);
            yield return new WaitForSeconds(timeUntilNextMeasure);
            curBeat = 1;
            EnableSoundPlayback();
        }
        
        void OnEnable()
        {
            puzzleGrid.OnAStartLinkUpdated += ScheduleSingleLink;
            puzzleGrid.OnAStartLinkFullyDestroyed += StopPlaybackForLink;
        }
        void OnDisable()
        {
            puzzleGrid.OnAStartLinkUpdated -= ScheduleSingleLink;
            puzzleGrid.OnAStartLinkFullyDestroyed -= StopPlaybackForLink;
        }

        void Start()
        {
            StartCoroutine(PlaybackLoop());

            // initiate chuck subsinstance
            myChuck = ChuckManager.Instance.chuckSubInstance;
            if (beatsInLoop > 0) Debug.LogWarning("Note: LinkPlaybackManager using pre-set beatsInLoop serialized inspector values");
            else beatsInLoop = LevelLoader.instance.GetBeatsInLoop();

            // must declare Chuck events before creating listeners
            // this is to coordinate reference beat playback
            myChuck.RunCode( string.Format( @"
                global Event playReference;
                global Event pauseReference;
            "));

            // intialize those events 
            referenceSound?.PlaySound();

            // if playback is already enabled in inspector, sync to metronome on start
            if (soundPlaybackEnabled)
            {
                soundPlaybackEnabled = false; // disable until sync completes
                StartCoroutine(SyncAndStart());
            }
        }
        #endregion

        public void SetMetronnomePlaying()
        {
            isMetronomePlaying = true;
        }

        public void SetMetronnomePaused()
        {
            isMetronomePlaying = false;
        }

        // playback of sounds in links.
        // also controls playback of metronome so that timing is the same
        private IEnumerator PlaybackLoop()
        {
            float nextBeatTime = Time.time;
            WaitUntil untilSchedulerHasSounds = new(DoesSchedulerHaveAnyScheduledBeat);
            bool didLastHaveBeat = false;

            while (true)
            {
                if (!soundPlaybackEnabled)
                {
                    nextBeatTime = Time.time;
                    yield return null;
                    continue;
                }

                WaitUntil wait = new(() => Time.time >= nextBeatTime);

                yield return wait;

                // if metronome is on, play metronome sound (at the right time)
                if (isMetronomePlaying) 
                {
                    if ((curBeat % 2) - 1 == 0)
                    {
                        myChuck.BroadcastEvent("playMetronomeSingleSound");   
                    }   
                }

                nextBeatTime = Time.time + secondsPerBeat;

                if (curBeat > beatsInLoop) { 
                    curBeat = 1; 
                }

                if (DoesSchedulerHaveAnyScheduledBeat()) {
                    if (!didLastHaveBeat)
                    {
                        didLastHaveBeat = true;
                        curBeat = 1;
                    }
                    foreach (var key in knownLinks.Keys)
                    {
                        // only play when there is actually links to play
                        if (knownLinks[key].scheduledBeats.Count < curBeat) continue; 
                        var currentLinkBeat = knownLinks[key].scheduledBeats[curBeat - 1];

                        knownLinks[key].link.IndexPlaySound(
                            currentLinkBeat.indexInLink, 
                            currentLinkBeat.firstInPiece,
                            secondsPerBeat,
                            currentLinkBeat.silent
                        );
                    }
                } else didLastHaveBeat = false;

                curBeat += 1;
                // NO yield return waitBeat here — WaitUntil at the top handles timing
            }
        }

        private bool DoesSchedulerHaveAnyScheduledBeat()
        {
            foreach (int key in knownLinks.Keys)
            {
                // Debug.Log($"examining key {key}; there are {knownLinks[key].scheduledBeats.Count} beats scheduled!");
                if (knownLinks[key].scheduledBeats.Count > 0) return true;
            }
            return false;
        }

        private void ParseLinks() // turns the list of grid-links into a list of "parsable" objects that are easier to play sound with 
        {
            puzzleGrid.GetAllLinks().ForEach((l) => ScheduleSingleLink(l));
        }

        private void ScheduleSingleLink(GridLink which)
        {
            int soundId = which.GetStartSoundIDIfExists();
            // Debug.Log($"Schedule Single Link for ID: {soundId}");
            if (soundId == -1) { 
                Debug.LogError($"Scheduled a link that is not connected to any start sound: IsStartLink {which.HasStartData()}, IsEndLink {which.HasEndData()}"); 
                return;
            }

            // reset gridlink if we know this link already
            if (knownLinks.ContainsKey(soundId))
            {
                knownLinks[soundId].link = which;
                knownLinks[soundId].scheduledBeats.Clear();
            } 
            else
            {
                knownLinks[soundId] = new()
                {
                    link = which, 
                    scheduledBeats = new()
                };
            }

            // setup the schedule 
            int indexInLink = 0;
            foreach (Piece p in which.GetPieces().Select(ld => ld.piece))
            {
                // Debug.Log("Scheduling piece");
                // need to schedule even if silent, but must indicate whether to play sound or not
                for (int i = 0; i < p.GetPieceTiles().Count; i++)
                {
                    // Debug.Log($"\tScheduling {i}.");
                    knownLinks[soundId].scheduledBeats.Add(
                        new() 
                        {
                            firstInPiece = i == 0, 
                            silent = p.IsSilentPiece(), 
                            indexInLink = indexInLink
                        });
                }
                indexInLink += 1;
            }
        }
        private void StopPlaybackForLink(int soundId)
        {
            // reset gridlink if we know this link already
            if (knownLinks.ContainsKey(soundId))
            {
                knownLinks.Remove(soundId);
            } 
        }

        public void DisableSoundPlayback() => soundPlaybackEnabled = false;

        // NOTE: if wanted to reset link playback, just move curBeat to global and set equal to one.
        public void EnableSoundPlayback() => soundPlaybackEnabled = true;
    }
}