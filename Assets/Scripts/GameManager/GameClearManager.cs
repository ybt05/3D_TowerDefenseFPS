// GameClearScene GameClearManagerObject
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ScoreEntry
{
    public int score;
    public string genre;
    public string timestamp;

    public ScoreEntry(int score, string genre)
    {
        this.score = score;
        this.genre = genre;
        this.timestamp = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }
}

public class GameClearManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;   // 最終スコア表示
    [SerializeField] private TextMeshProUGUI rewardText;  // 報酬アクセサリー表示
    [SerializeField] private Button titleButton;          // タイトルへ戻るボタン

    // スコア履歴保存用データ
    public class ScoreData
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    private void Start()
    {
        // マウスカーソルを表示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // タイトルへ戻るボタンのイベント登録
        titleButton.onClick.AddListener(BackToTitle);
        ResultProcessor.GrantRewardAccessories(); // Wave数に応じたアクセサリー付与
        ResultProcessor.GrantCoins();             // スコアに応じたコイン付与
        ResultProcessor.SaveAccessories();        // アクセサリー保存
        // スコア計算
        float finalScore = ResultProcessor.CalculateFinalScore();
        scoreText.text = $"スコア: {finalScore:F0}";
        // スコア履歴保存
        ResultProcessor.SaveScoreHistory();
        // 報酬アクセサリー表示
        ShowRewardAccessories();
    }
    // 報酬アクセサリー一覧をUIに表示
    private void ShowRewardAccessories()
    {
        var list = MainManager.instance.ownedAccessories;
        int count = ResultProcessor.GetRewardCount();
        int start = Mathf.Max(0, list.Count - count);
        string result = "";
        for (int i = start; i < list.Count; i++)
        {
            result += ResultProcessor.FormatAccessoryText(list[i]) + "\n\n";
        }
        rewardText.text = result;
    }
    // タイトルへ戻る
    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}