using System;
using System.Collections.Generic;
using GamePieces;
using Unity.VisualScripting;
using UnityEngine;

namespace PuzzleGrid
{
    // TOOD: what I am doing with this? how do I used it
    // TODO: check the edge case that 
    // TODO: create a merge links function and then also a split links function (this will come a LOT later ... like next week)
    // TODO: there should also be some kind of link manager
    public class GridLink
    {
        // list of pieces 
        [SerializeField] private List<Piece> pieces = new();
    

        // add Piece to link    
        public void AddPiece(Piece piece)
        {
            pieces.Add(piece);
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

        // TODO: split links (and then delete if the size is only one)
        // NOTE: there should never be a link with only one Piece in it ...

        // determine if Piece exists within Link
        public bool ContainsPiece(Piece piece)
        {
            return pieces.Contains(piece);
        } 

    }
}