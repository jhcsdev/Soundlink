using System;
using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    [Serializable]
    public class LinkData 
    {
        [SerializeField] private Vector2 startPos;
        [SerializeField] private Vector2 endPos;

        [SerializeField] private int soundID;

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
    }
}
