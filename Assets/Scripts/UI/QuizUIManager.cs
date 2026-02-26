// MainScene Canvas/TowerPanel/QuizPanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class QuizUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject quizPanel;         // クイズ問題パネル
    [SerializeField] private GameObject explanationPanel;  // 解説パネル
    [SerializeField] private GameObject mainPanel;         // タワーパネル

    [Header("Buttons")]
    [SerializeField] private Button backButton;            // 戻るボタン
    [SerializeField] private Button quitButton;            // クイズ終了ボタン
    [SerializeField] private Button nextButton;            // 次の問題へ
    [SerializeField] private Button firstButtonOnMainPanel; // 戻った時に選択されるボタン
    [SerializeField] private Button firstButtonOnQuizPanel; // 次の問題開始時に選択されるボタン

    [Header("Quiz UI")]
    [SerializeField] private TextMeshProUGUI questionText; // 問題文
    [SerializeField] private RawImage questionImage;       // 問題画像
    [SerializeField] private Button[] answerButtons;       // 選択肢ボタン

    [Header("Explanation UI")]
    [SerializeField] private TextMeshProUGUI explanationText; // 解説文
    [SerializeField] private RawImage explanationImage;       // 解説画像

    [Header("Tower")]
    [SerializeField] private Tower tower; // クイズ成功/失敗でタワーに影響を与える場合に使用

    private QuizManager quizManager;      // クイズデータ管理
    private QuizData currentQuiz;         // 現在の問題

    void Start()
    {
        // QuizManager を取得
        quizManager = FindFirstObjectByType<QuizManager>();
        if (quizManager == null)
        {
            Debug.Log("QuizUIManager.cs QuizManagerが見つかりません");
            return;
        }
        // ボタンイベント登録
        backButton.onClick.AddListener(OnBack);
        quitButton.onClick.AddListener(OnQuit);
        nextButton.onClick.AddListener(OnNextQuiz);
        // ジャンルを MainManager から取得
        string genre = MainManager.instance != null ? MainManager.instance.getGenre() : "typing";
        // クイズデータ初期化
        quizManager.Initialize(genre);
        // 最初の問題を表示
        ShowNextQuiz();
    }
    // 次のクイズを表示
    public void ShowNextQuiz()
    {
        EnemySpawner.instance.QuizExplanation(false);
        // ランダムでクイズを取得
        currentQuiz = quizManager.GetRandomQuiz();
        if (currentQuiz == null)
        {
            questionText.text = "クイズがみつかりません";
            questionImage.gameObject.SetActive(false);
            return;
        }
        // UI切り替え
        quizPanel.SetActive(true);
        explanationPanel.SetActive(false);
        // 問題文・画像を表示
        questionText.text = currentQuiz.question;
        questionImage.texture = currentQuiz.questionImage;
        questionImage.gameObject.SetActive(currentQuiz.questionImage != null);
        // 選択肢をランダムに並び替え
        List<string> options = new List<string>(currentQuiz.wrongAnswers);
        options.Add(currentQuiz.correctAnswer);
        options = options.OrderBy(x => Random.value).ToList();
        // ボタンに選択肢を設定
        for (int i = 0; i < answerButtons.Length; i++)
        {
            string answer = options[i];
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answer;
            // 古いイベントを削除して新しいイベントを登録
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answer));
        }
    }
    // 選択肢がクリックされたとき
    void OnAnswerSelected(string selected)
    {
        bool isCorrect = quizManager.CheckAnswer(selected);
        // UI切り替え
        explanationPanel.SetActive(true);
        quizPanel.SetActive(false);
        EnemySpawner.instance.QuizExplanation(true);
        // 解説画像
        explanationImage.texture = currentQuiz.explanationImage;
        explanationImage.gameObject.SetActive(currentQuiz.explanationImage != null);
        // 正解・不正解処理
        if (isCorrect)
        {
            explanationText.text = $"正解\n{currentQuiz.explanation}";
            PlayerHealth.instance.ReinforcementPlayer(); // プレイヤー強化
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        }
        else
        {
            explanationText.text = $"不正解\n{currentQuiz.explanation}";
            EnemySpawner.instance.TimerControl(); // 残り時間を減らす
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
        }
        // 次へボタンを選択状態に
        EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
    }
    // 次の問題へ
    public void OnNextQuiz()
    {
        ShowNextQuiz();
        EventSystem.current.SetSelectedGameObject(firstButtonOnQuizPanel.gameObject);
    }
    // クイズ終了
    public void OnQuit()
    {
        ShowNextQuiz(); // 次回のために問題を更新しておく
        mainPanel.SetActive(true);
        quizPanel.SetActive(false);
        explanationPanel.SetActive(false);
        EnemySpawner.instance.QuizExplanation(false);
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }

    // 戻る
    public void OnBack()
    {
        mainPanel.SetActive(true);
        quizPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }
}