using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private int scorePerKill = 100;

    public int Score { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UpdateUI();
    }

    public void AddKillScore()
    {
        AddScore(scorePerKill);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        Score += amount;
        UpdateUI();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    public void SetScoreText(Text text)
    {
        scoreText = text;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText == null)
            return;

        scoreText.text = "Score: " + Score;
    }
}
