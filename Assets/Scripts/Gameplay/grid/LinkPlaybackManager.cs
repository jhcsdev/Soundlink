using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamePieces;
using PuzzleGrid;
using UnityEngine;
using TrackSounds;

namespace GridLinks
{
    [RequireComponent(typeof(PuzzleGrid.PuzzleGrid))]
    public class LinkPlaybackManager : MonoBehaviour
    {
        public static LinkPlaybackManager Instance;
        [SerializeField] TrackSound metronome;

        [SerializeField] float bpm;
        [SerializeField, Tooltip("number of beats for the playback loop")] private int beatsInLoop;
        private float secondsPerBeat;
        private PuzzleGrid.PuzzleGrid puzzleGrid;

        [SerializeField] private bool soundPlaybackEnabled;

        private Dictionary<int /*soundid*/, SchedulerInformation> knownLinks = new();
        private Dictionary<int /*soundid*/, int /*currentschedulerindex*/> scheduleIndexTracker = new();
        private class SchedulerInformation { public List<EventTiming> scheduledBeats; public GridLink link; }
        private class EventTiming { public int beat; public bool silent; }

        #region unity functions
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            puzzleGrid = GetComponent<PuzzleGrid.PuzzleGrid>();
            if (beatsInLoop == 0) Debug.LogWarning("loop beats 0 in link playback");
            secondsPerBeat = 60 / bpm / 4;
            // TODO: should these be sixteenth notes? 
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
        }
        #endregion

        private IEnumerator PlaybackLoop()
        {
            WaitForSeconds waitBeat = new(secondsPerBeat); 
            WaitUntil untilSchedulerHasSounds = new(DoesSchedulerHaveAnyScheduledBeat);
            int curBeat = 1;

            while(true)
            {
                if (!soundPlaybackEnabled)
                {
                    yield return null;
                    continue;
                }

                if (!DoesSchedulerHaveAnyScheduledBeat()) {
                    Debug.Log("No sounds in scheduler!");
                    yield return untilSchedulerHasSounds; // wait until there's actually something to play
                }

                // check if curBeat exceeds loop; if it does, restart the loop
                if (curBeat > beatsInLoop) { 
                    curBeat = 1; 
                    foreach(var key in scheduleIndexTracker.Keys.ToList()) 
                    {
                        Debug.Log($"reset schedule of {key}"); 
                        scheduleIndexTracker[key] = 0;
                    }
                }

                Debug.Log($"Beat: {curBeat}");

                // shift up to the maximum beat we can for each known beat
                foreach (var key in knownLinks.Keys)
                {
                    if (!scheduleIndexTracker.ContainsKey(key)) scheduleIndexTracker[key] = 0;

                    while(scheduleIndexTracker[key] < knownLinks[key].scheduledBeats.Count && knownLinks[key].scheduledBeats[scheduleIndexTracker[key]].beat <= curBeat)
                    {
                        if (knownLinks[key].scheduledBeats[scheduleIndexTracker[key]].beat == curBeat)
                        {
                            knownLinks[key].link.IndexPlaySound(
                                scheduleIndexTracker[key], 
                                knownLinks[key].scheduledBeats[scheduleIndexTracker[key]].silent
                            );
                        }

                        scheduleIndexTracker[key] += 1;
                        if(!knownLinks[key].link.HasStartData()) { 
                            Debug.LogWarning("beware: there is a grid link that made it to the scheduler without having a start link!"); 
                            break; 
                        } 
                    }
                }

                // play metronome sound (kind of hacky tbh ...)
                // TODO: sync the mhould make sure that we are synced, right?
                // if (curBeat % 2 == 0)
                // {
                //     metronome?.PlaySound();   
                // }

                yield return waitBeat;
                curBeat += 1;                       
            }
        }

        private bool DoesSchedulerHaveAnyScheduledBeat()
        {
            foreach (int key in knownLinks.Keys)
            {
                Debug.Log($"examining key {key}; there are {knownLinks[key].scheduledBeats.Count} beats scheduled!");
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
            Debug.Log($"Schedule Single Link for ID: {soundId}");
            if (soundId == -1) { 
                Debug.LogError($"Scheduled a link that is not connected to any start sound: IsStartLink {which.HasStartData()}, IsEndLink {which.HasEndData()}"); 
                return;
            }

            // reset gridlink if we know this link already
            if (knownLinks.ContainsKey(soundId))
            {
                knownLinks[soundId].link = which;
                knownLinks[soundId].scheduledBeats.Clear();

                if (!scheduleIndexTracker.ContainsKey(soundId)) { 
                    Debug.LogWarning($"knownlinks has id {soundId} but scheduler missing it"); 
                }else scheduleIndexTracker.Remove(soundId); // fully removing it will cause a fast-speedup in the playback loop.
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
            int curBeat = 1;
            foreach (Piece p in which.GetPieces())
            {
                // need to schedule even if silent, but must indicate whether to play sound or not
                knownLinks[soundId].scheduledBeats.Add(new() {beat = curBeat, silent = p.IsSilentPiece()});

                curBeat += p.GetPieceTiles().Count;
            }
        }
        private void StopPlaybackForLink(int soundId)
        {
            // reset gridlink if we know this link already
            if (knownLinks.ContainsKey(soundId))
            {
                knownLinks.Remove(soundId);
                scheduleIndexTracker.Remove(soundId);
            } 
        }

        private int BinarySearchForId<T>(T forItem, List<T> searchIn) where T : IComparable<T>
        {
            int top = searchIn.Count;
            int bottom = 0;
            int half = bottom + (top - bottom) / 2;

            while (top - bottom > 1 && searchIn[half].CompareTo(forItem) != 0)
            {
                if (searchIn[half].CompareTo(forItem) > 0) top = half;
                else bottom = half + 1;

                half = bottom + (top-bottom) / 2;
            }

            return half;
        }

        public void DisableSoundPlayback() => soundPlaybackEnabled = false;

        // NOTE: if wanted to reset link playback, just move curBeat to global and set equal to one.
        public void EnableSoundPlayback() => soundPlaybackEnabled = true;

        public int GetLoopSize() => beatsInLoop;
    }
}