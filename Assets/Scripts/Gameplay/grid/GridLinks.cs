using System;
using System.Collections.Generic;
using System.Linq;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    [Serializable]
    public class GridLink
    {
        // list of pieces 
        [SerializeField] private List<LinkData> pieces = new();

        [NonSerialized] private LinkPlacementData startPlacementData = null;
        [NonSerialized] private LinkPlacementData endPlacementData = null;
        private bool hasStartLinkRef = false;
        private bool hasEndLinkRef = false;

        #region Getters
        public List<LinkData> GetPieces()
        {
            return pieces;
        }
        public LinkPlacementData GetStartPlacementData()
        {
            return startPlacementData;
        }
        public LinkPlacementData GetEndPlacementData()
        {
            return endPlacementData;
        }
        /// <summary>
        /// gets the sound id of the start link 
        /// </summary>
        /// <returns></returns>
        public int GetStartSoundIDIfExists()
        {
            if (HasStartData()) return startPlacementData.GetSoundID();
            return -1;
        }
        /// <summary>
        /// inverse of above
        /// </summary>
        /// <returns>sound id or -1 if does not exist</returns>
        public int GetEndSoundIDIfExists()
        {
            if (HasEndData()) return endPlacementData.GetSoundID();
            return -1;
        }
        #endregion

        #region setters
        public GridLink SetEndPlacementData(LinkPlacementData data)
        {
            Debug.Log("setting end");
            if (data == null) { ResetEndPlacementData(); return this; }

            endPlacementData = data;
            hasEndLinkRef = true;
            ForceRejoinOnPieces(endPlacementData);
            return this;
        }
        public GridLink ResetEndPlacementData()
        {
            if (!hasEndLinkRef && endPlacementData == null) return this;
            hasEndLinkRef = false;
            endPlacementData = null; 
            if (!HasStartData()) LostLinkData();
            return this;
        }

        public GridLink SetStartPlacementData(LinkPlacementData data)
        {
            Debug.Log("setting start");
            if (data == null) { ResetStartPlacementData(); return this; }

            startPlacementData = data;
            hasStartLinkRef = true;
            ForceRejoinOnPieces(startPlacementData);
            return this; 
        }
        public GridLink ResetStartPlacementData()
        {
            if (!hasStartLinkRef && startPlacementData == null) return this;
            hasStartLinkRef = false;
            startPlacementData = null;
            if (!HasEndData()) LostLinkData();
            return this;
        }
        #endregion

        #region predicates
        // determine if Piece exists within Link
        public bool ContainsPiece(Piece piece)
        {
            return pieces.Any(ld => ld.piece == piece);
        } 

        public bool HasStartData() => hasStartLinkRef;
        public bool HasEndData() => hasEndLinkRef;
        /// <summary>
        /// a link is "live" when it either 1) has more than two pieces or 2) is a Start or End piece.
        /// </summary>
        /// <returns></returns>
        public bool IsLinkLive() => HasEndData() || HasStartData() || pieces.Count >= 2;

        /// <summary>
        /// evaluates whether two links are compatible with one another
        /// </summary>
        /// <param name="other">the link to compare against</param>
        /// <returns>
        /// false if - both links have end data, both links have start data, or links have opposite start/end & soundIds do not match.
        /// true otherwise.
        /// </returns>
        public bool IsOtherCompatible(GridLink other)
        {
            if ((this.HasEndData() && other.HasEndData()) || (this.HasStartData() && other.HasStartData())) return false;
            if (this.HasStartData() && other.HasEndData()) return this.GetStartSoundIDIfExists() == other.GetEndSoundIDIfExists();
            if (this.HasEndData() && other.HasStartData()) return this.GetEndSoundIDIfExists() == other.GetStartSoundIDIfExists();

            return true;
        }
        /// <summary>
        /// returns true if this link has both a start and end link and the sound id of those references matches
        /// </summary>
        /// <returns></returns>
        public bool IsLinkComplete()
        {
            return HasStartData() && HasEndData() && (startPlacementData.GetSoundID() == endPlacementData.GetSoundID());
        }
        #endregion 

        #region major functionality
        /// <summary>
        /// Merge another link with this one, re-ordering pieces and resetting start/end link data when necessary.
        /// </summary>
        /// <param name="other">The link to merge with.</param>
        /// <param name="otherPivot">The "piece" in other to base the merge on.</param>
        /// <param name="otherPivotTile">The PieceTile of otherPivot that connects to thisPivot.</param>
        /// <param name="thisPivot">The "piece" in this to base the merge on.</param>
        /// <param name="thisPivotTile">The PieceTile of thisPivot that connects to otherPivot.</param>
        /// <returns>True on success, false on failure. See IsOtherCompatible() for definition of compatibility</returns>
        public bool MergeLink(GridLink other, Piece otherPivot, PieceTile otherPivotTile, Piece thisPivot, PieceTile thisPivotTile)
        {
            // these checks remove situations where: the "other" is complete, "this" is complete, "other" & "this" both are start / end, or "other" & "this" have different soundIds 
            if (this.IsLinkComplete())
            {
                Debug.LogWarning($"Tried to merge other: {other} with this: {this}, but this is complete.");
                return false;
            }
            if (!IsOtherCompatible(other))
            {
                Debug.LogWarning($"Tried to merge {this} with incompatible link {other}"); 
                return false;
            }

            List<LinkData> otherPieces = other.GetPieces();

            // re-orient otherPieces so that "pivot" is always at the start of the otherPieces list. 
            // todo:: (thought) we can allow connecting to the middle of an existing piece by splitting "other"; however, this adds some other edge cases that we'd need to think through logically...
            int otherPivotIndex = -1;
            for (int i = 0; i < otherPieces.Count; i++)
            {
                if (otherPieces[i].piece == otherPivot) { otherPivotIndex = i; break; }
            }

            // reorient list
            Debug.Log($"Reorienting other by index found at {otherPivotIndex}");
            if (otherPivotIndex == otherPieces.Count - 1) otherPieces.Reverse();
            else if (otherPivotIndex != 0) 
            { 
                Debug.LogWarning("(unimplemented) Tried to merge with a piece that was in the middle of an existing link (or pivot was not in link)"); 
                return false;
            }

            // in this case, "other" should go after "this"  (this = Start, other = End)
            if (other.HasEndData() || this.HasStartData())
            {
                Debug.Log("Other has end data, or this has start data!");

                LinkData thisPivotLD = pieces.FirstOrDefault(ld => ld.piece == thisPivot);
                if (thisPivotLD != null) thisPivotLD.endTile = thisPivotTile;
                else Debug.LogWarning("MergeLink (branch 1): could not find thisPivot LinkData to set endTile.");

                otherPieces[0].startTile = otherPivotTile;

                pieces.AddRange(otherPieces);
                if (other.HasEndData()) SetEndPlacementData(other.GetEndPlacementData());
            }
            // in this case, "this" should go after "other"  (other = Start, this = End)
            else if (other.HasStartData() || this.HasEndData())
            {
                Debug.Log("Other has start data, and this has end data!");

                otherPieces.Reverse();
                otherPieces[otherPieces.Count - 1].endTile = otherPivotTile;

                LinkData thisPivotLD = pieces.FirstOrDefault(ld => ld.piece == thisPivot);
                if (thisPivotLD != null)
                    thisPivotLD.startTile = thisPivotTile;
                else
                    Debug.LogWarning("MergeLink (branch 2): could not find thisPivot LinkData to set startTile.");

                otherPieces.AddRange(pieces);
                pieces = otherPieces;
                if (other.HasStartData()) SetStartPlacementData(other.GetStartPlacementData());
            }
            // in this case, it is a free-standing link. We don't know the end direction of the link, so we're gonna have to just combine the two, preserving existing ordering
            else
            {
                int thisPivotIndex = -1;
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i].piece == thisPivot) { thisPivotIndex = i; break; }
                }
                if (thisPivotIndex == -1) { Debug.LogWarning("Could not find thisPivot in pieces."); return false; }

                pieces.InsertRange(thisPivotIndex, otherPieces);
            }

            // this updates the pieces visually when multiple pieces are connected
            if (HasStartData() || HasEndData()) ForceRejoinOnPieces(HasStartData() ? startPlacementData : endPlacementData);

            return true;
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
                if (pieces[i].piece != at) continue;


                if (i == 0 || i == pieces.Count - 1) 
                {
                    Debug.Log($"Split link - edge case (index {i}, {pieces.Count} pieces). Removing piece.");
                    // edge case - need to update start/end fields.
                    if (i == 0) ResetStartPlacementData();
                    if (i == pieces.Count - 1) ResetEndPlacementData();

                    RemovePiece(at);
                    return true;
                }

                // otherwise, need to create a new grid link. 
                Debug.Log($"Split link - found piece; making new grid link.");
                end = new();
                List<LinkData> newLinkPieces = pieces.Skip(i + 1).Take(pieces.Count - i - 1).ToList();

                end.pieces.AddRange(newLinkPieces); // preserves startTile & endTile

                pieces = pieces.Take(i).ToList();

                end.SetEndPlacementData(endPlacementData);
                ResetEndPlacementData(); // note:: setting endLinkReference to null here is okay only under the assumption that links will never continue past the end position. Otherwise, we cannot assume this.

                Debug.Log($"Split link successful. This link is now: {start}. \n The new link is: {end}.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// adds piece to the proper order based on a set of conditions - if start, add after basedOn, if end, add before basedOn, todo: if complete, don't add (?)
        /// </summary>
        /// <param name="toAdd">the piece to add to the link</param>
        /// <param name="toAddTile">the tile of toAdd that basedOnTile is connected to</param>
        /// <param name="basedOn" (nullable)>the piece that toAdd is connected to; will be used to base insertion position</param>
        /// <param name="basedOnTile" (nullable)>the tile of basedOn that toAddTile is connected to</param>
        /// <returns>this if basedOn is null or successfully added piece; null if could not find basedOn</returns>
        public GridLink AddPiece(Piece toAdd, PieceTile toAddTile, Piece basedOn, PieceTile basedOnTile, bool creatingStartLink = false)
        {
            Debug.Log("adding piece");
            if (basedOn == null) { 
                Debug.Log($"Adding piece to {this} without basedOn. Ensure order!");
                pieces.Add(
                    new()
                    { 
                        piece = toAdd, 
                        startTile = creatingStartLink ? toAddTile : null, 
                        endTile = creatingStartLink ? null : toAddTile, 
                    }
                ); 
                if (HasStartData() || HasEndData()) toAdd.JoinLink(HasStartData() ? startPlacementData : endPlacementData, toAddTile);
                return this;
            }
            Debug.Log($"Adding piece {toAdd.name} based on {basedOn.name}");

            // add according to basedOn
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i].piece == basedOn)
                {
                    if (HasEndData()) 
                    { 
                        // add BEFORE basedOn; this means that basedOnTile is the startTile of basedOn and toAddTile is endTile of toAdd
                        pieces[i].startTile = basedOnTile;
                        pieces.Insert(i, new()
                        {
                            piece = toAdd, endTile = toAddTile
                        });  
                    } 
                    else {
                        // add AFTER basedOn; this means that basedOnTile is the endTile of basedOn and toAddTile is  startTile of toAdd
                        pieces[i].endTile = basedOnTile;
                        pieces.Insert(i+1, new()
                        {
                            piece = toAdd, startTile = toAddTile
                        }); 
                    }

                    if (HasStartData() || HasEndData()) 
                    {
                        Debug.Log($"The link is a start or end. \n {this}");
                        toAdd.JoinLink(HasStartData() ? startPlacementData : endPlacementData, toAddTile);
                    }

                    return this;
                }
            }
            return null;
        }

        // remove Piece from link.
        // returns number of elements left in link
        public int RemovePiece(Piece piece)
        {
            pieces.RemoveAll(ld => ld.piece == piece);
            return pieces.Count;
        }

        public void IndexPlaySound(int index, bool isFirstInPiece, bool silent = false)
        {
            if (index < 0 || index > pieces.Count) { Debug.LogWarning($"Link passed index {index}, which is out of bounds for piece count {pieces.Count}"); return; }

            if (!silent && isFirstInPiece) GetStartPlacementData().GetTrackSound().PlaySound();

            pieces[index].piece.LinkPulse(startPlacementData.GetPulseColor(), pieces[index].startTile, isFirstInPiece);
        }

        #endregion

        #region changing piece state

        private void ForceRejoinOnPieces(LinkPlacementData associatedData)
        {
            Debug.Log("would be rejoining...");
            // pieces.ForEach(item => item.piece.JoinLink(HasStartData() ? startPlacementData : endPlacementData));
        }
        private void LostLinkData()
        {
            pieces.ForEach(item => item.piece.LeaveLink());
        }

        #endregion

        #region other
        public override string ToString()
        {
            string baseStr = base.ToString();
            baseStr += $"\n LinkStart: {startPlacementData} \n\t StartId: {startPlacementData?.GetSoundID()} \n LinkEnd: {endPlacementData} \n\tEndId: {endPlacementData?.GetSoundID()}  \n Number pieces: {pieces.Count}. \n ";
            return baseStr;
        }
        #endregion
    }

    [Serializable]
    public class LinkData
    {
        public PieceTile startTile;
        public Piece piece;
        public PieceTile endTile; // nullable
    }
}