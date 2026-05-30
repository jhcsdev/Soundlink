using Inventory;
using PuzzleGrid;
using StreamEvents;
using TrackSounds;
using UnityEngine;

namespace SceneTransition
{
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader instance;
        [SerializeField] private FloatEventStream OnLevelCompleted;

        [SerializeField, Tooltip("The level that will be loaded during OnEnable(). Note that, from the menu scene, this gets set at runtime, and the object carries over between scenes. For testing in LevelScene, you should set the value here.")]
        private LevelData activeLevel;
        private int furthestCompletedLevel = 0;

        void Awake()
        {
            if (instance == null)
            {
                instance = this; 
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        void OnEnable()
        {
            OnLevelCompleted.Sub(LevelComplete);
        }
        void OnDisable()
        {
            OnLevelCompleted.Unsub(LevelComplete);
        }

        private void LevelComplete(float what)
        {
            if (what > furthestCompletedLevel) furthestCompletedLevel = (int)what;
            else return;
        }
        public int GetFurthestCompleteLevel() => furthestCompletedLevel;

        public void LoadLevel(LevelData what)
        {
            activeLevel = what;
            AsyncSceneLoader.Instance.LoadLevelScene();
        }
        public void CloseLevel()
        {
            activeLevel = null;
            AsyncSceneLoader.Instance.LoadMainScene();
        }
        public Vector4 GetGridFitShape()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return Vector4.one; } 
            return activeLevel.fitGridBetween;
        }
        public int GetBeatsInLoop()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return 0; } 
            return activeLevel.numBeatsInLoop;
        }

        public GridData GetGridData()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return null; } 
            if (activeLevel.grid == null) { Debug.LogError("Level loader has level, but is missing the grid data"); return null; }

            return activeLevel.grid;
        }

        public InventoryData GetInventoryData()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return null; } 
            if (activeLevel.inventory == null) { Debug.LogError("Level loader has level, but is missing the inventory data"); return null; }

            return activeLevel.inventory;
        }
        public float GetLevel()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return -1; } 
            return activeLevel.level;
        }
        public TrackSound GetReferencePlayback()
        {
            if (activeLevel == null) { Debug.LogError("Level Loader needs an active level."); return null; } 
            if (activeLevel.referenceBeatTrackSound == null) { Debug.LogError("Level loader has level, but is missing the inventory data"); return null; }

            return activeLevel.referenceBeatTrackSound;
        }
    }
}