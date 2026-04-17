using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    // TODO: create a merge links function and then also a split links function (this will come a LOT later ... like next week)
    // todo - need to introduce some method of ordering GridLinks
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
            return endLinkReference;
        }

        public bool HasStartData() => startLinkReference != null;
        public bool HasEndData() => endLinkReference != null;
        /// <summary>
        /// gets the sound id of the start link 
        /// </summary>
        /// <returns></returns>
        public int GetStartSoundIDIfExists()
        {
            if (HasStartData()) return startLinkReference.GetSoundID();
            else return -1;
        }
        /// <summary>
        /// returns true if this link has both a start and end link and the sound id of those references matches
        /// </summary>
        /// <returns></returns>
        public bool IsLinkComplete()
        {
            return HasStartData() && HasEndData() && (startLinkReference.GetSoundID() == endLinkReference.GetSoundID());
        }

    }
}