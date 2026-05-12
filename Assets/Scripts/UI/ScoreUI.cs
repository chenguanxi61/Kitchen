using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private TMP_Text scoreTextTMP;

    private void Awake()
    {
        if (scoreText == null && scoreTextTMP == null)
        {
            scoreText = GetComponentInChildren<Text>(true);
            scoreTextTMP = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void Start()
    {
        if (DeliverManager.Instance != null)
        {
            DeliverManager.Instance.OnScoreChanged += DeliverManager_OnScoreChanged;
        }

        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (DeliverManager.Instance != null)
        {
            DeliverManager.Instance.OnScoreChanged -= DeliverManager_OnScoreChanged;
        }
    }

    private void DeliverManager_OnScoreChanged()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        int score = DeliverManager.Instance != null ? DeliverManager.Instance.GetScore() : 0;
        string scoreString = score.ToString();

        if (scoreText != null)
        {
            scoreText.text = scoreString;
        }

        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scoreString;
        }
    }
}
