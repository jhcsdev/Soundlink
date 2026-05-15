using SceneTransition;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class LevelButtonPresser : MonoBehaviour
    {
        void Start()
        {
            int furthestCompleteLevel = LevelLoader.instance.GetFurthestCompleteLevel();
            for(int i = 0; i < transform.childCount; i++)
            {
                if (i >furthestCompleteLevel) transform.GetChild(i).GetComponent<Button>().interactable = false;
                else transform.GetChild(i).GetComponent<Button>().interactable = true;
            }
        }
    }
}
