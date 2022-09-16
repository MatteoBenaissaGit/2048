using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUIController : MonoBehaviour
{
    [BoxGroup("EndGame")] [SerializeField] private string _winMessage;
    [BoxGroup("EndGame")] [SerializeField] private string _loseMessage;

    [BoxGroup("Referencing")] [SerializeField] private TextMeshProUGUI _textMesh;
    [BoxGroup("Referencing")] [SerializeField] private Image _blackBackground;
    [BoxGroup("Referencing")] [SerializeField] private GameObject _nextButton;

    private void Start()
    {
        HideEndGameUI();
    }

    private void HideEndGameUI()
    {
        gameObject.SetActive(false);
        Color color = new Color(0, 0, 0, 0);
        _blackBackground.color = color;
        _textMesh.transform.localScale = Vector3.zero;
        _nextButton.transform.localScale = Vector3.zero;;
    }

    public void LaunchEndGameUI(bool isWin)
    {
        LevelUnlockedScript.Level++;
        gameObject.SetActive(true);
        const float animDuration = 1f;
        const float blackFadeAlpha = .6f;
        _textMesh.text = isWin ? _winMessage : _loseMessage;
        _blackBackground.DOFade(blackFadeAlpha, animDuration);
        _textMesh.transform.DOScale(1, animDuration);
        StartCoroutine(ShowButton());
    }

    private IEnumerator ShowButton()
    {
        const float waitTime = 2f;
        const float animDuration = .5f;
        yield return new WaitForSeconds(waitTime);
        //show button
        _nextButton.transform.DOScale(1, animDuration);
    }

    private void GoToSelectionScene()
    {
        //go to selection scene
    }
}
