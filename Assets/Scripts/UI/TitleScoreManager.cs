// TitleScene Canvas/ScorePanel
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject scorePanel;        // スコア一覧パネル
    [SerializeField] private GameObject mainPanel;         // タイトルメインパネル
    [SerializeField] private Transform scoreListParent;    // スクロールビューのContent
    [SerializeField] private GameObject scoreItemPrefab;   // スコア1件分の表示プレハブ
    [SerializeField] private Button backButton;            // 戻るボタン
    [SerializeField] private Button firstButtonOnTitlePanel; // 戻った時に選択されるボタン

    // スコア1件分のデータ
    [System.Serializable]
    public class ScoreEntry
    {
        public int score;       // スコア
        public string genre;    // ジャンル（literacy / typing）
        public string timestamp; // プレイ日時
    }

    // スコア全体データ
    [System.Serializable]
    public class ScoreData
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }
    public void Start()
    {
        ShowScorePanel();                 // 起動時にスコア一覧を表示
        backButton.onClick.AddListener(OnBack);
    }
    // 暗号化されたスコアデータを読み込む
    private ScoreData LoadScoreEncrypted()
    {
        if (!PlayerPrefs.HasKey("scoreData")) { return new ScoreData(); } // データが無ければ空のリストを返す
        string encrypted = PlayerPrefs.GetString("scoreData");
        string json = CryptoUtility.Decrypt(encrypted); // 復号化
        return JsonUtility.FromJson<ScoreData>(json);
    }
    // ジャンル名を日本語に変換
    private string ConvertGenreToJapanese(string genre)
    {
        switch (genre.ToLower())
        {
            case "literacy": return "リテラシー";
            case "typing": return "タイピング";
            default: return genre;
        }
    }

    // スコア一覧をUIに表示
    public void ShowScorePanel()
    {
        ScoreData data = LoadScoreEncrypted();
        // 既存の表示をクリア
        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }
        // スコア1件ずつUI生成
        foreach (ScoreEntry entry in data.entries)
        {
            string genreJp = ConvertGenreToJapanese(entry.genre);
            GameObject item = Instantiate(scoreItemPrefab, scoreListParent);
            item.GetComponent<TextMeshProUGUI>().text =
                $"スコア: {entry.score}  {genreJp}  {entry.timestamp}";
        }
        scorePanel.SetActive(true);
    }

    // タイトル画面へ戻る
    void OnBack()
    {
        scorePanel.SetActive(false);
        mainPanel.SetActive(true);
        // ゲームパッド操作時の初期選択
        EventSystem.current.SetSelectedGameObject(firstButtonOnTitlePanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
    }
}