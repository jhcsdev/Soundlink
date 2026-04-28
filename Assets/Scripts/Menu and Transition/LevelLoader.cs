using Inventory;
using PuzzleGrid;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

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
        if (activeLevel == null) { Debug.LogWarning("Level Loader missing an active level."); return null; } 
        if (activeLevel.grid == null) { Debug.LogWarning("Level loader has level, but is missing the grid data"); return null; }

        return activeLevel.grid;
    }

    public InventoryData GetInventoryData()
    {
        if (activeLevel == null) { Debug.LogWarning("Level Loader missing an active level."); return null; } 
        if (activeLevel.inventory == null) { Debug.LogWarning("Level loader has level, but is missing the inventory data"); return null; }

        return activeLevel.inventory;
    }
}
