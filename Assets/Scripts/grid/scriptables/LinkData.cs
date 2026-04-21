using System;
using System.Collections.Generic;
using GamePieces;
using TrackSounds;
using UnityEngine;

namespace PuzzleGrid
{
    [Serializable]
    public class LinkPlacementData 
    {
        [SerializeField] private Vector2 startPos;
        [SerializeField] private Vector2 endPos;

        [SerializeField] private TrackSound playsSound;
        [SerializeField] private int soundID;

        [SerializeField] private Color startLinkColor;
        [SerializeField] private Color endLinkColor;

        public Vector2 GetStartPos()
        {
            return startPos;
        }

        // should the link manager be here?

        public Vector2 GetEndPos()
        {
            return endPos;
        }

        public int GetSoundID()
        {
            return soundID;
        }

        public TrackSound GetTrackSound() => playsSound;
    }
}
