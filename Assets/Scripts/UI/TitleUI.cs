// TitleScene TitleManagerObject
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;        // タイトルのメインメニュー
    [SerializeField] private GameObject accessoryPanel;   // アクセサリー装備パネル（ゲーム開始前）
    [SerializeField] private GameObject difficultyPanel;  // 難易度選択パネル
    [SerializeField] private GameObject genrePanel;       // ジャンル選択パネル
    [SerializeField] private GameObject scorePanel;       // スコア一覧パネル
    [SerializeField] private GameObject settingPanel;     // 設定パネル
    [SerializeField] private GameObject gachaPanel;       // ガチャパネル

    [Header("Buttons")]
    [SerializeField] private Button startButton;          // ゲーム開始
    [SerializeField] private Button scoreButton;          // スコア画面へ
    [SerializeField] private Button settingButton;        // 設定画面へ
    [SerializeField] private Button gachaButton;          // ガチャ画面へ
    [SerializeField] private Button quitButton;           // ゲーム終了

    [SerializeField] private Button firstButtonOnAccessoryPanel; // アクセサリー画面の初期選択
    [SerializeField] private Button firstButtonOnScorePanel;     // スコア画面の初期選択
    [SerializeField] private Button firstButtonOnSettingPanel;   // 設定画面の初期選択
    [SerializeField] private Button firstButtonOnGachaPanel;     // ガチャ画面の初期選択

    private void Awake()
    {
        // タイトル起動時に表示するパネルを初期化
        mainPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        accessoryPanel.SetActive(false);
        genrePanel.SetActive(false);
        scorePanel.SetActive(false);
        settingPanel.SetActive(false);
        gachaPanel.SetActive(false);
    }

    private void Start()
    {
        // ボタンイベント登録
        startButton.onClick.AddListener(StartGame);
        scoreButton.onClick.AddListener(Score);
        settingButton.onClick.AddListener(Setting);
        gachaButton.onClick.AddListener(Gacha);
        quitButton.onClick.AddListener(Quit);
        // 保存された音量を反映
        float volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        AudioListener.volume = volume;
    }

    // ゲーム開始
    public void StartGame()
    {
        accessoryPanel.SetActive(true);
        mainPanel.SetActive(false);
        settingPanel.SetActive(false);
        gachaPanel.SetActive(false);

        // ゲーム開始前の状態をリセット
        MainManager.instance.ResetGameState();
        // ゲームパッド用の初期選択
        EventSystem.current.SetSelectedGameObject(firstButtonOnAccessoryPanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // スコア画面へ
    public void Score()
    {
        mainPanel.SetActive(false);
        scorePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButtonOnScorePanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // 設定画面へ
    public void Setting()
    {
        mainPanel.SetActive(false);
        settingPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButtonOnSettingPanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // ガチャ画面へ
    public void Gacha()
    {
        mainPanel.SetActive(false);
        gachaPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButtonOnGachaPanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // ゲーム終了
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}