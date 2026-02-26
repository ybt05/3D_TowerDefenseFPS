// TitleScene Canvas/GenrePanel
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GenreSelector : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject difficultyPanel; // 難易度選択パネル
    [SerializeField] private GameObject genrePanel;      // 現在のジャンル選択パネル
    [SerializeField] private GameObject equipPanel;      // アクセサリー装備パネル

    [Header("Buttons")]
    [SerializeField] private Button selectLiteracyButton;      // 読解ジャンル選択
    [SerializeField] private Button selectTypingButton;        // タイピングジャンル選択
    [SerializeField] private Button backButton;                // 戻るボタン
    [SerializeField] private Button firstButtonOnAccessoryPanel; // 戻った時に選択されるボタン
    [SerializeField] private Button firstButtonOnDifficultyPanel; // 次画面で最初に選択されるボタン

    private void Start()
    {
        // ジャンル選択ボタンにイベント登録
        selectLiteracyButton.onClick.AddListener(() => SelectGenre("literacy"));
        selectTypingButton.onClick.AddListener(() => SelectGenre("typing"));
        // 戻るボタン
        backButton.onClick.AddListener(OnBack);
    }

    // ジャンルを選択 → 難易度選択画面へ
    public void SelectGenre(string genre)
    {
        // 選択したジャンルを保存
        MainManager.instance.setGenre(genre);
        // 決定音
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        // パネル切り替え
        difficultyPanel.SetActive(true);
        genrePanel.SetActive(false);
        // ゲームパッド操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnDifficultyPanel.gameObject);
    }
    // アクセサリー装備画面へ戻る
    public void OnBack()
    {
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
        genrePanel.SetActive(false);
        equipPanel.SetActive(true);
        // ゲームパッド操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnAccessoryPanel.gameObject);
    }
}