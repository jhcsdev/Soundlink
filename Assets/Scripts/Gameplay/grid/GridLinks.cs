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
            if (data == null) { ResetEndPlacementData(); return this; }

            endPlacementData = data;
            hasEndLinkRef = true;
            if (pieces != null && pieces.Count > 0) FullChainRejoin(pieces[^1].endTile, data);
            return this;
        }
        public GridLink ResetEndPlacementData()
        {
            if (!hasEndLinkRef && endPlacementData == null) return this;
            hasEndLinkRef = false;
            endPlacementData = null; 
            if (!HasStartData()) LostLinkData(null);
            return this;
        }

        public GridLink SetStartPlacementData(LinkPlacementData data)
        {
            if (data == null) { ResetStartPlacementData(); return this; }

            startPlacementData = data;
            hasStartLinkRef = true;
            if (pieces != null && pieces.Count > 0) FullChainRejoin(pieces[0].startTile, data);
            return this; 
        }
        public GridLink ResetStartPlacementData()
        {
            if (!hasStartLinkRef && startPlacementData == null) return this;
            hasStartLinkRef = false;
            startPlacementData = null;
            if (!HasEndData()) LostLinkData(null);
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
            if (otherPieces == null || otherPieces.Count == 0 || pieces.Count == 0) return false;

            // find the indices of joining pieces
            int thisPivotIndex = pieces.FindIndex(ld => ld.piece == thisPivot);
            int otherPivotIndex = otherPieces.FindIndex(ld => ld.piece == otherPivot);

            if (thisPivotIndex == -1 || otherPivotIndex == -1) return false;

            bool thisIsFirst = thisPivotIndex == 0;
            bool thisIsLast = thisPivotIndex == pieces.Count - 1;
            
            if (!thisIsFirst && !thisIsLast && pieces.Count > 1) 
            {
                Debug.LogWarning("thisPivot is in the middle of the chain");
                return false;
            }

            // merge flow: true if [pieces] -> [otherPieces], false if [otherPieces] -> [pieces]
            bool isThisToOther = false;

            if (this.HasStartData() || other.HasEndData()) 
            {
                isThisToOther = true;
            } 
            else if (this.HasEndData() || other.HasStartData()) 
            {
                isThisToOther = false;
            } 
            else 
            {
                if (thisIsLast) isThisToOther = true;
                else if (thisIsFirst) isThisToOther = false;
            }

            bool rejoinAlreadyPerformed = false;

            if (isThisToOther)
            {
                // [pieces] -> [otherPieces]

                if (thisPivotIndex == 0 && pieces.Count > 1) 
                {
                    ReverseLinkData(pieces);
                }

                if (otherPivotIndex == otherPieces.Count - 1 && otherPieces.Count > 1) 
                {
                    ReverseLinkData(otherPieces);
                } 
                else if (otherPivotIndex != 0 && otherPieces.Count > 1) 
                {
                    Debug.LogWarning("otherPivot is in middle of chain");
                    return false;
                }

                pieces[^1].endTile = thisPivotTile;
                otherPieces[0].startTile = otherPivotTile;

                pieces.AddRange(otherPieces);

                if (other.HasEndData()) 
                {
                    SetEndPlacementData(other.GetEndPlacementData());
                    rejoinAlreadyPerformed = true;
                }
            }
            else
            {
                // [otherPieces] -> [pieces]
                
                if (otherPivotIndex == 0 && otherPieces.Count > 1) 
                {
                    ReverseLinkData(otherPieces);
                } 
                else if (otherPivotIndex != otherPieces.Count - 1 && otherPieces.Count > 1) 
                {
                    Debug.LogWarning("otherPivot is in the middle of chain");
                    return false;
                }

                if (thisPivotIndex == pieces.Count - 1 && pieces.Count > 1) 
                {
                    ReverseLinkData(pieces);
                }

                otherPieces[^1].endTile = otherPivotTile;
                pieces[0].startTile = thisPivotTile;

                otherPieces.AddRange(pieces);
                pieces = otherPieces;

                if (other.HasStartData()) 
                {
                    SetStartPlacementData(other.GetStartPlacementData());
                    rejoinAlreadyPerformed = true;
                }
            }

            if (!rejoinAlreadyPerformed && (HasStartData() || HasEndData())) FullChainRejoin(thisPivotTile, HasStartData() ? startPlacementData : endPlacementData);

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

                PieceTile atTile = pieces[i].startTile != null ? pieces[i].startTile : pieces[i].endTile;
                bool isFirst = i == 0;
                bool isLast  = i == pieces.Count - 1;

                if (isFirst || isLast)
                {
                    if (HasStartData() || HasEndData()) at.LeaveLink(atTile);

                    if (isFirst && pieces.Count > 1) pieces[1].startTile = null;
                    if (isLast && pieces.Count > 1) pieces[i - 1].endTile = null;

                    RemovePiece(at);

                    if (isFirst) ResetStartPlacementData();
                    if (isLast)  ResetEndPlacementData();

                    if (HasStartData() && pieces.Count > 0) FullChainRejoin(pieces[0].startTile, startPlacementData);
                    else if (HasEndData() && pieces.Count > 0) FullChainRejoin(pieces[^1].endTile, endPlacementData);

                    return true;
                }

                // middle-piece case
                end = new();
                List<LinkData> newLinkPieces = pieces.Skip(i + 1).Take(pieces.Count - i - 1).ToList();
                end.pieces.AddRange(newLinkPieces);
                pieces = pieces.Take(i).ToList();

                if (pieces.Count > 0) pieces[^1].endTile = null;
                if (end.pieces.Count > 0) end.pieces[0].startTile = null;

                at.LeaveLink(atTile);
                
                end.SetEndPlacementData(endPlacementData);
                
                if (!end.HasEndData() && !end.HasStartData())
                {
                    end.LostLinkData(null);
                }

                ResetEndPlacementData();

                if (HasStartData() && pieces.Count > 0) FullChainRejoin(pieces[0].startTile, startPlacementData);

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
            if (IsLinkComplete()) return this;

            if (basedOn == null) 
            { 
                pieces.Add(new() { 
                    piece = toAdd, 
                    startTile = creatingStartLink ? toAddTile : null, 
                    endTile = creatingStartLink ? null : toAddTile 
                }); 
                if (HasStartData() || HasEndData()) toAdd.JoinLink(HasStartData() ? startPlacementData : endPlacementData, toAddTile);
                return this;
            }

            int i = pieces.FindIndex(ld => ld.piece == basedOn);
            if (i == -1) return null;

            bool insertBefore = false;

            // insert the piece before or after basedOn?
            if (HasEndData()) 
            { 
                insertBefore = true; 
            } 
            else if (HasStartData()) 
            {
                insertBefore = false;
            }
            else
            {
                if (i == 0) insertBefore = true;
                else if (i == pieces.Count - 1) insertBefore = false;
                else 
                {
                    Debug.LogWarning($"AddPiece: Attempted to add to the middle of a link at index {i}. Defaulting to AFTER.");
                    insertBefore = false;
                }
            }

            if (insertBefore)
            {
                pieces[i].startTile = basedOnTile;
                pieces.Insert(i, new() { piece = toAdd, endTile = toAddTile });  
            }
            else
            {
                pieces[i].endTile = basedOnTile;
                pieces.Insert(i + 1, new() { piece = toAdd, startTile = toAddTile }); 
            }

            if (HasStartData() || HasEndData()) 
            {
                toAdd.JoinLink(HasStartData() ? startPlacementData : endPlacementData, toAddTile);
            }

            return this;
        }

        // remove Piece from link.
        // returns number of elements left in link
        public int RemovePiece(Piece piece)
        {
            pieces.RemoveAll(ld => ld.piece == piece);
            return pieces.Count;
        }

        public void IndexPlaySound(int index, bool isFirstInPiece, float secondsPerBeat, bool silent = false)
        {
            if (index < 0 || index >= pieces.Count) { Debug.LogWarning($"Link passed index {index}, which is out of bounds for piece count {pieces.Count}"); return; }

            if (!silent && isFirstInPiece) GetStartPlacementData().GetTrackSound().PlaySound();

            pieces[index].piece.LinkPulse(startPlacementData.GetPulseColor(), pieces[index].startTile, isFirstInPiece, secondsPerBeat);
        }

        #endregion

        #region changing piece state

        private void FullChainRejoin(PieceTile tile, LinkPlacementData associatedData)
        {
            pieces.ForEach(item => item.piece.JoinLink(associatedData, tile));
            // Debug.Log("would be rejoining...");
            // pieces.ForEach(item => item.piece.JoinLink(HasStartData() ? startPlacementData : endPlacementData));
        }
        private void LostLinkData(PieceTile tile)
        {
            pieces.ForEach(item => item.piece.LeaveLink(tile));
        }

        #endregion

        #region other
        public override string ToString()
        {
            string baseStr = base.ToString();
            baseStr += $"\n LinkStart: {startPlacementData} \n\t StartId: {startPlacementData?.GetSoundID()} \n LinkEnd: {endPlacementData} \n\tEndId: {endPlacementData?.GetSoundID()}  \n Number pieces: {pieces.Count}. \n ";
            return baseStr;
        }

        private void ReverseLinkData(List<LinkData> list)
        {
            list.Reverse();
            foreach (var ld in list)
            {
                PieceTile temp = ld.startTile;
                ld.startTile = ld.endTile;
                ld.endTile = temp;
            }
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