using System;
using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    [CreateAssetMenu(fileName = "GridData", menuName = "Gameplay/Grid Data")]
    public class GridData : ScriptableObject
    {
        public int width;
        public int height;
        // todo - this should probably be a custom editor, for now it's just a list of pieces
        // that start on the grid at a certain position. no guarantees that initial pieces dont collide.
        public List<Tuple<Vector2, PieceData>> initialPieces;
    }
}
