using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    [CreateAssetMenu(fileName = "PieceData", menuName = "Gameplay/Piece Data")]
    public class PieceData : ScriptableObject
    {
        // i think this script needs a custom editor -- will do that later
        [SerializeField, Tooltip("pieces will be offset from the first piecetile in the list")] 
        private List<PieceTileCoordinate> pieces;
    }

    [Serializable]
    public struct PieceTileCoordinate
    {
        Vector2 coord;
        PieceTile tile;
        bool isPlaceableEdge; // for the "edge" parts of the piece (start and end)
    }
}