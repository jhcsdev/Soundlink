using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PauseMenuController))]
public class PauseMenuVisuals : MonoBehaviour
{
    #region animation configs
    private static float fadeInTime = 0.2f;
    private static float fadeOutTime = 0.2f;
    private static Color backdropColor = new(0.1f, 0.1f, 0.1f, 0.7f);
    private static Color invisibleColor = new(0, 0, 0, 0);
    private static float sweepInTime = 0.8f;
    private static float sweepOutTime = 0.8f;
    #endregion

    #region animation objects
    [SerializeField] Image backdropFadeScreen;
    [SerializeField] RectTransform pauseMainRoot;
    [SerializeField] RectTransform howToPlayRoot;
    [SerializeField] TMP_Text pauseTitleText;
    [SerializeField] List<RectTransform>  sweepInObjects;
    #endregion

    #region Sequences
    private Sequence currentFadeSequence;
    #endregion

    private PauseMenuController pauseMenu; 

    void Awake()
    {
        pauseMenu = GetComponent<PauseMenuController>();
    }

    void OnEnable()
    {
        pauseMenu.PauseMenuStateChanged += PauseChanged;
        pauseMenu.SubmenuStateChange += PauseSubmenuChange;
    }

    void OnDisable()
    {
        pauseMenu.PauseMenuStateChanged -= PauseChanged;
        pauseMenu.SubmenuStateChange -= PauseSubmenuChange;
    }

    void PauseChanged(bool isPaused) { if(isPaused) { OpenPauseMenu(); } else ClosePauseMenu(); }

    void OpenPauseMenu()
    {
        if (currentFadeSequence != null && currentFadeSequence.active)
        {
            currentFadeSequence.Kill();
        }
        currentFadeSequence = DOTween.Sequence().AppendCallback(
            () => backdropFadeScreen.gameObject.SetActive(true)
        ).Append(
            backdropFadeScreen.DOColor(backdropColor, fadeInTime)
        );
    }
    void ClosePauseMenu()
    {
        if (currentFadeSequence != null && currentFadeSequence.active)
        {
            currentFadeSequence.Kill();
        }

        currentFadeSequence = DOTween.Sequence().Append(
            backdropFadeScreen.DOColor(invisibleColor, fadeOutTime)
        ).AppendCallback(
            () => backdropFadeScreen.gameObject.SetActive(false)
        );
    }

    void PauseSubmenuChange(PauseMenuSubState state)
    {
        switch (state)
        {
            case PauseMenuSubState.MAIN_PAUSE_MENU:
                CloseHowToPlayMenuObjects(OpenPauseMenuObjects);
                break;
            case PauseMenuSubState.HOW_TO_PLAY:
                ClosePauseMenuObjects(OpenHowToPlayMenuObjects);
                break;
            case PauseMenuSubState.ALL_CLOSED:
                CloseHowToPlayMenuObjects(ClosePauseMenuObjects);
                break;
        }
    }

    void OpenPauseMenuObjects()
    {
        pauseMainRoot.gameObject.SetActive(true);
    }
    void ClosePauseMenuObjects(Action subsequentOpen = null)
    {
        pauseMainRoot.gameObject.SetActive(false);
        subsequentOpen?.Invoke();
    }
    void ClosePauseMenuObjects() => ClosePauseMenuObjects(null);
    void OpenHowToPlayMenuObjects()
    {
        howToPlayRoot.gameObject.SetActive(true);
    }
    void CloseHowToPlayMenuObjects(Action subsequentOpen = null)
    {
        howToPlayRoot.gameObject.SetActive(false);
        subsequentOpen?.Invoke();
    }
    void CloseHowToPlayMenuObjects() => CloseHowToPlayMenuObjects(null);
}

