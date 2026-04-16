using System;
using System.Collections.Generic;
using GamePieces;
using Unity.VisualScripting;
using UnityEngine;

namespace PuzzleGrid
{
    [CreateAssetMenu(fileName = "GridData", menuName = "Gameplay/Grid Data")]
    public class GridData : ScriptableObject
    {
        public int width;
        public int height;

        public List<LinkPlacementData> linkDatas;

        // TODO: define current ID (start at 0)
        public int currentSoundID = 0;

        // get all the link info based on position!
        public GridTileType GetTileInfo(int x, int y, out int soundID) 
        {             
            // Debug.Log("X: " + x);
            // Debug.Log("Y: " + y);
            // check if matches any start or end tile
            foreach(LinkPlacementData link in linkDatas)
            {
                Vector2 startPos = link.GetStartPos();
                Vector2 endPos = link.GetEndPos();

                // if either start or end, return the SOUNDID!
                if (x == startPos.x && y == startPos.y)
                {
                    soundID = link.GetSoundID();
                    return GridTileType.START;
                }

                if (x == endPos.x && y == endPos.y)
                {
                    soundID = link.GetSoundID();
                    return GridTileType.END;
                }
            }

            // otherwise, just basic
            soundID = -1;
            return GridTileType.BASIC;
        }
    }

    public enum GridTileType
    {
        BASIC, 
        START,
        END, 
    }
}
