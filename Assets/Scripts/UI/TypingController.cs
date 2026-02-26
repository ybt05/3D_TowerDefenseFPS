// MainScene TowerCanvas/TowerPanel/TypingPanel
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TypingController : MonoBehaviour
{
    [Header("Typing Strings")]
    [SerializeField] private List<string> easyTypingStrings = new List<string>();   // Easy用の単語リスト
    [SerializeField] private List<string> normalTypingStrings = new List<string>(); // Normal用の単語リスト
    [SerializeField] private List<string> hardTypingStrings = new List<string>();   // Hard用の単語リスト

    [Header("UI")]
    [SerializeField] private Transform textParent;          // 文字を1文字ずつ並べる親オブジェクト
    [SerializeField] private TextMeshProUGUI textPrefab;    // 1文字表示用のプレハブ
    [SerializeField] private TextMeshProUGUI timerText;     // 残り時間表示

    [Header("Panels")]
    [SerializeField] private GameObject typingPanel;        // タイピングパネル
    [SerializeField] private GameObject mainPanel;          // 戻り先のメインパネル

    [Header("Buttons")]
    [SerializeField] private Button backButton;             // 戻るボタン
    [SerializeField] private Button firstButtonOnMainPanel; // 戻った時に選択されるボタン

    [Header("Time Limit")]
    [SerializeField] private float timeLimit = 20f;         // 1問の制限時間

    private float timer;                                    // 現在の残り時間
    private List<TextMeshProUGUI> charList = new List<TextMeshProUGUI>(); // 表示中の文字リスト
    private int currentCharIndex = 0;                       // 現在入力すべき文字のインデックス
    private string currentString;                           // 現在の問題文字列
    private int score = 0;                                  // 正解数
    private Color normalColor = new Color(1, 1, 1, 1f);     // 通常色
    private Color correctColor = new Color(0.2f, 1, 0.2f, 1f); // 正解色
    private Color wrongColor = new Color(1, 0.2f, 0.2f, 1f);   // 不正解色
    private string difficulty;                              // 難易度
    private bool flag = false;                              // 強化処理の重複防止フラグ

    void Start()
    {
        backButton.onClick.AddListener(OnBack);
        // 難易度取得
        difficulty = MainManager.instance != null ? MainManager.instance.getDifficulty() : "Easy";
        if (difficulty != "Easy" && difficulty != "Normal" && difficulty != "Hard")
        {
            difficulty = "Easy";
        }
        // Hardは制限時間を5倍にする
        if (difficulty == "Hard")
        {
            timeLimit *= 5f;
        }
        LoadRandomString();
    }
    void Update()
    {
        if (string.IsNullOrEmpty(currentString)) { return; }
        // タイマー更新
        timer -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = timer.ToString("F1");
        }
        // 時間切れ → ペナルティ & 次の問題へ
        if (timer <= 0f)
        {
            EnemySpawner.instance.TimerControl(); // Wave残り時間を減らす
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
            LoadRandomString();
        }
        // 全文字入力済みなら処理しない
        if (currentCharIndex >= currentString.Length) { return; }
        // 入力取得
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) { return; }
        char key = input[0];
        // 正解判定
        if (key == currentString[currentCharIndex])
        {
            charList[currentCharIndex].color = correctColor;
            currentCharIndex++;
            // 文字列をすべて入力し終えた
            if (currentCharIndex >= currentString.Length)
            {
                score++;
                AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
                LoadRandomString();
                flag = true;
            }
            // プレイヤー強化処理
            if (difficulty == "Hard" && flag)
            {
                PlayerHealth.instance.ReinforcementPlayer();
                flag = false;
            }
            else if (score % 5 == 0 && score != 0 && flag)
            {
                PlayerHealth.instance.ReinforcementPlayer();
                flag = false;
            }
        }
        else
        {
            // 不正解
            charList[currentCharIndex].color = wrongColor;
            // Normal / Hard はペナルティで時間減少
            if (difficulty == "Hard" || difficulty == "Normal")
            {
                timer -= 1f;
                if (timer < 0) { timer = 0.1f; }
            }
        }
    }

    // ランダムな文字列を読み込み、UIを再生成
    private void LoadRandomString()
    {
        if (difficulty == "Easy")
        {
            currentString = easyTypingStrings[Random.Range(0, easyTypingStrings.Count)];
        }
        else if (difficulty == "Normal")
        {
            currentString = normalTypingStrings[Random.Range(0, normalTypingStrings.Count)];
        }
        else if (difficulty == "Hard")
        {
            currentString = hardTypingStrings[Random.Range(0, hardTypingStrings.Count)];
        }
        else
        {
            currentString = normalTypingStrings[Random.Range(0, normalTypingStrings.Count)];
        }
        // タイマーリセット
        timer = timeLimit;
        // UIクリア
        foreach (Transform child in textParent)
        {
            Destroy(child.gameObject);
        }
        charList.Clear();
        currentCharIndex = 0;
        // 文字を1文字ずつ生成
        foreach (char c in currentString)
        {
            var t = Instantiate(textPrefab, textParent);
            t.text = (c == ' ') ? "␣" : c.ToString(); // スペースは可視化
            t.color = normalColor;
            charList.Add(t);
        }
    }
    // メインパネルへ戻る
    public void OnBack()
    {
        mainPanel.SetActive(true);
        typingPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }
}