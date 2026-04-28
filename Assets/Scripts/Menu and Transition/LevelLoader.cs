using Inventory;
using PuzzleGrid;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    [SerializeField, Tooltip("The level that will be loaded during OnEnable(). Note that, from the menu scene, this gets set at runtime, and the object carries over between scenes. For testing in LevelScene, you should set the value here.")]
    private LevelData activeLevel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void LoadLevel(LevelData what)
    {
        activeLevel = what;
        SceneManager.LoadScene("LevelScene"); // todo:: would prefer abstracting scenemanager into some sort of utility class handling transition visuals
    }
    public void CloseLevel()
    {
        activeLevel = null;
        SceneManager.LoadScene("MainScene"); // todo:: see LoadLevel() LoadScene todo
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
}
