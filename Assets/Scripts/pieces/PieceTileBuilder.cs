using UnityEngine;

namespace GamePieces
{
    public class PieceTileBuilder : MonoBehaviour
    {
        public static PieceTileBuilder Instance;


        void Awake()
        {
            if (Instance != null) Instance = this;
            else Destroy(gameObject);
        }

        public PieceTile CreateTile()
        {
            // todo
            return null;
        }
    }
}