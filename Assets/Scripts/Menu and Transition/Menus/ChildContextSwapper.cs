using UnityEngine;

namespace Menus
{
    public class ChildContextSwapper : MonoBehaviour
    {
        // sets all children of this object to disabled, except for the one specified as "to"
        public void SwapChild(string to)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == to)transform.GetChild(i).gameObject.SetActive(true);
                else transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
