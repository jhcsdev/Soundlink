

using Inventory;
using PuzzleGrid;
using TrackSounds;
using UnityEngine;

// to allow wrapping grids and inventories into the same object; arguably, grids and inventories should be combined into this object
// to remove the need to have this wrapper object at all, but for now, leaving the powers separated is fine
[CreateAssetMenu(fileName ="LevelData", menuName ="Gameplay/Level Data")]
public class LevelData : ScriptableObject
{
    public GridData grid;
    public InventoryData inventory;
    public int level;
    public int numBeatsInLoop;
    [Tooltip("Xmin, Xmax, Ymin, Ymax -- in SCREEN SPACE")]
    public Vector4 fitGridBetween;
    public TrackSound referenceBeatTrackSound;
}