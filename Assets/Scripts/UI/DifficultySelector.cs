// TitleScene Canvas/DifficultyPanel
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DifficultySelector : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject difficultyPanel; // 難易度選択パネル
    [SerializeField] private GameObject genrePanel;      // ジャンル選択パネル（戻る時に表示）

    [Header("Buttons")]
    [SerializeField] private Button selectTutorialButton;   // チュートリアル
    [SerializeField] private Button selectEasyButton;       // イージー
    [SerializeField] private Button selectNormalButton;     // ノーマル
    [SerializeField] private Button selectHardButton;       // ハード
    [SerializeField] private Button backButton;             // 戻るボタン
    [SerializeField] private Button firstButtonOnGenrePanel; // 戻った時に最初に選択されるボタン

    private void Start()
    {
        // 各難易度ボタンにクリックイベントを登録
        selectTutorialButton.onClick.AddListener(() => SelectDifficulty("Tutorial"));
        selectEasyButton.onClick.AddListener(() => SelectDifficulty("Easy"));
        selectNormalButton.onClick.AddListener(() => SelectDifficulty("Normal"));
        selectHardButton.onClick.AddListener(() => SelectDifficulty("Hard"));
        // 戻るボタン
        backButton.onClick.AddListener(OnBack);
    }

    // 難易度を選択してゲーム開始
    public void SelectDifficulty(string difficulty)
    {
        // 決定音
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        // 選択した難易度を保存
        MainManager.instance.setDifficulty(difficulty);
        // メインシーンへ遷移
        SceneManager.LoadScene("MainScene");
    }

    // ジャンル選択画面へ戻る
    public void OnBack()
    {
        // キャンセル音
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
        // パネル切り替え
        difficultyPanel.SetActive(false);
        genrePanel.SetActive(true);
        // コントローラー操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnGenrePanel.gameObject);
    }
}