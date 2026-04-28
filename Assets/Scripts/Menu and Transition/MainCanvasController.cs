

using UnityEngine;

public class MainMenuCanvasController : MonoBehaviour
{
    public void TriggerLevelTransition(LevelData forLevel)
    {
        Debug.Log("Loading level!");
        LevelLoader.instance.LoadLevel(forLevel);
    }
}