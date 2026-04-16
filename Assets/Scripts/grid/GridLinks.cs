using System.Collections.Generic;
using GamePieces;
using Unity.Android.Gradle.Manifest;
using UnityEditor.UI;
using UnityEngine;

namespace PuzzleGrid
{
    // TODO: create a merge links function and then also a split links function (this will come a LOT later ... like next week)
    public class GridLink
    {
        // list of pieces 
        [SerializeField] private List<Piece> pieces = new();

        private LinkPlacementData startLinkReference;
        private LinkPlacementData endLinkReference;

        // add Piece to link; returns self  
        public GridLink AddPiece(Piece piece)
        {
            pieces.Add(piece);
            return this;
        }

        // remove Piece from link.
        // returns number of elements left in link
        public int RemovePiece(Piece piece)
        {
            pieces.Remove(piece);
            return pieces.Count;
        }

        public List<Piece> GetPieces()
        {
            return pieces;
        }

        // TODO: delete link?

        // TODO: merge links
        // note here - when merging two links, you need to compare the start / end link references. 
        // i'd say that if both start + end

        // TODO: split links (and then delete if the size is only one)
        // NOTE: there should never be a link with only one Piece in it ...

        // determine if Piece exists within Link
        public bool ContainsPiece(Piece piece)
        {
            return pieces.Contains(piece);
        } 

        public GridLink SetStartPlacementData(LinkPlacementData data)
        {
            startLinkReference = data;
            return this; 
        }

        public LinkPlacementData GetStartPlacementData()
        {
            return startLinkReference;
        }

        public GridLink SetEndPlacementData(LinkPlacementData data)
        {
            endLinkReference = data;
            return this;
        }
        public LinkPlacementData GetEndPlacementData()
        {
            return startLinkReference;
        }

    }
}