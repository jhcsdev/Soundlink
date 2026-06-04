using SceneTransition;
using UnityEngine;

namespace Menus
{
    public class MainMenuCanvasController : MonoBehaviour
    {
        public void TriggerLevelTransition(LevelData forLevel)
        {
            Debug.Log("Loading level!");
            LevelLoader.instance.LoadLevel(forLevel);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}