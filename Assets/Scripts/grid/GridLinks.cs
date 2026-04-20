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

        // 
        // adds piece to the proper order based on a set of conditions - if start, add after basedOn, if end, add before basedOn, todo: if complete, don't add (?)
        /// <summary>
        /// add Piece to link in an ordered position
        /// </summary>
        /// <param name="toAdd">the piece to add to the link</param>
        /// <param name="basedOn" (nullable)>the piece that toAdd is connected to; will be used to base insertion position</param>
        /// <returns>this if basedOn is null or successfully added piece; null if could not find basedOn</returns>
        public GridLink AddPiece(Piece toAdd, Piece basedOn)
        {
            if (basedOn == null) { 
                if (pieces.Count == 0) pieces.Add(toAdd); 
                else Debug.LogWarning("Tried adding a piece to a link without any 'basedOn' parameter, but there are existing pieces in the link!");
                return this;
            }

            // add according to basedOn
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == basedOn)
                {
                    if (HasEndData()) { pieces.Insert(i, toAdd);  } // add BEFORE basedOn
                    else pieces.Insert(i+1, toAdd); // add AFTER basedOn

                    return this;
                }
            }
            return null;
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

        /// <summary>
        /// merge links
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public GridLink MergeLink(GridLink other)
        {
            // todo - merge links!
            return this;
        }

        /// <summary>
        /// Splits this link into two by removing the "at" piece. 
        /// </summary>
        /// <param name="at">The piece to orchestrate the split around.</param>
        /// <param name="start">The first "half" of the newly-split link. Should be "this" link.</param>
        /// <param name="end">The second half of the link, newly created. Might be null, if "at" piece is the first / last piece in the link - then this function is just a wrapper for RemovePiece().</param>
        /// <returns>True if the "at" piece exists, and was removed. False otherwise.</returns>
        public bool SplitLink(Piece at, out GridLink start, out GridLink end)
        {
            start = this;
            end = null;

            if (pieces.Count <= 0) return false;

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != at) continue;
                if (i == 0 || i == pieces.Count - 1) 
                {
                    // edge case - need to update start/end fields.
                    return RemovePiece(at);
                }
            }
        }

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

        public void IndexPlaySound(int index)
        {
            if (index < 0 || index > pieces.Count) { Debug.LogWarning($"Link passed index {index}, which is out of bounds for piece count {pieces.Count}"); return; }



            GetStartPlacementData().GetTrackSound().PlaySound();
        }

    }
}