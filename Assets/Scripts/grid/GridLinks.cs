using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// adds piece to the proper order based on a set of conditions - if start, add after basedOn, if end, add before basedOn, todo: if complete, don't add (?)
        /// </summary>
        /// <param name="toAdd">the piece to add to the link</param>
        /// <param name="basedOn" (nullable)>the piece that toAdd is connected to; will be used to base insertion position</param>
        /// <returns>this if basedOn is null or successfully added piece; null if could not find basedOn</returns>
        public GridLink AddPiece(Piece toAdd, Piece basedOn)
        {
            if (basedOn == null) { 
                Debug.Log($"Adding piece to {this} without basedOn. Ensure order!");
                pieces.Add(toAdd); 
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


        
        public GridLink MergeLink(GridLink other)
        {
            // List<Piece> otherPieces = other.GetPieces();
            // if (other.Is)
            // return this;
        }

        /// <summary>
        /// Splits this link into two by REMOVING!! the "at" piece. 
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
                    Debug.Log($"Split link - edge case (index {i}, {pieces.Count} pieces). Removing piece.");
                    // edge case - need to update start/end fields.
                    if (i == 0) startLinkReference = null;
                    if (i == pieces.Count) endLinkReference = null;

                    RemovePiece(at);
                    return true;
                }

                // otherwise, need to create a new grid link. 
                Debug.Log($"Split link - found piece; making new grid link.");
                end = new();
                List<Piece> newLinkPieces = pieces.Skip(i + 1).Take(pieces.Count - i - 1).ToList();

                foreach (var p in newLinkPieces) end.AddPiece(p, null); 

                end.SetEndPlacementData(endLinkReference);
                endLinkReference = null; // note - setting endLinkReference to null here is okay only under the assumption that links will never continue past the end position. Otherwise, we cannot assume this.

                Debug.Log($"Split link successful. This link is now: {start}. \n The new link is: {end}.");
                return true;
            }

            return false;
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

        public override string ToString()
        {
            string baseStr = base.ToString();
            baseStr += $"\n LinkStart: {startLinkReference} \n LinkEnd: {endLinkReference} \n Number pieces: {pieces.Count}";
            return baseStr;
        }

    }
}