using SceneTransition;
using StreamEvents;
using UIFX;
using UnityEngine;

[RequireComponent(typeof(MultiMenuPageTransitioner))]
public class LevelCompleteMenu : MonoBehaviour
{
    [SerializeField] private FloatEventStream levelCompleteStream;

    [SerializeField] private IUITransitionElement winScreen;
    private MultiMenuPageTransitioner controller;

    void Awake()
    {
        controller = GetComponent<MultiMenuPageTransitioner>();
    }

    void OnEnable()
    {
        levelCompleteStream.Sub(LevelComplete);
    }
    void OnDisable()
    {
        levelCompleteStream.Unsub(LevelComplete);   
    }

    void LevelComplete(float _)
    {
        controller.GoToPage(winScreen);
    }
    public void OnReturnPressed()
    {
        // Debug.Log("return pressed");
        AsyncSceneLoader.Instance.LoadMainScene();
    }
}
