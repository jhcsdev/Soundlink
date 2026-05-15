using SceneTransition;
using StreamEvents;
using UnityEngine;

public class LevelCompleteMenu : MonoBehaviour
{
    [SerializeField] private FloatEventStream levelCompleteStream;
    [SerializeField] private RectTransform levelCompleteRoot;


    void OnEnable()
    {
        levelCompleteStream.Sub(LevelComplete);
        levelCompleteRoot.gameObject.SetActive(false);
    }
    void OnDisable()
    {
        levelCompleteStream.Unsub(LevelComplete);   
    }

    void LevelComplete(float _)
    {
        levelCompleteRoot.gameObject.SetActive(true); // todo:: improve
    }
    public void OnReturnPressed()
    {
        Debug.Log("return pressed");
        AsyncSceneLoader.Instance.LoadMainScene();
    }
}
