// TitleScene Canvas/SettingPanel
// MainScene Canvas/SettingPanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SettingManager : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Slider volumeSlider;            // 音量調整スライダー
    [SerializeField] private TextMeshProUGUI volumeValueText; // 音量の数値表示

    [Header("Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;        // 視点感度スライダー
    [SerializeField] private TextMeshProUGUI sensitivityValueText; // 感度の数値表示

    [Header("Username")]
    [SerializeField] private TMP_InputField usernameInputField; // ユーザー名入力欄
    private const string UsernameKey = "Username";

    [Header("Buttons")]
    [SerializeField] private Button backButton;               // 戻るボタン
    [SerializeField] private Button gameOverButton;           // ゲームオーバーへ遷移するボタン
    [SerializeField] private Button firstButtonOnTitlePanel;  // 戻った時に選択されるボタン

    [Header("UI Panels")]
    [SerializeField] private GameObject settingPanel;         // 設定パネル
    [SerializeField] private GameObject mainPanel;            // タイトル or メインパネル

    [Header("Player")]
    [SerializeField] private Transform player;                // プレイヤー参照

    private bool isOpened = false;                            // 設定パネルが開いているか
    public static SettingManager instance;
    private PlayerController playerController;                // プレイヤー操作制御

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 保存された設定値を読み込み
        float volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.1f);
        // ボタンイベント登録
        if (backButton != null) { backButton.onClick.AddListener(OnBack); }
        if (gameOverButton != null) { gameOverButton.onClick.AddListener(OnGameOver); }
        // スライダー初期値設定
        volumeSlider.value = volume;
        sensitivitySlider.value = sensitivity;
        // UI反映
        UpdateVolumeUI(volume);
        UpdateSensitivityUI(sensitivity);
        // スライダー変更時の処理
        volumeSlider.onValueChanged.AddListener(UpdateVolumeUI);
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivityUI);
        // ユーザー名の初期化
        if (usernameInputField != null)
        {
            string username = PlayerPrefs.GetString(UsernameKey, "");
            usernameInputField.text = username;
            usernameInputField.onEndEdit.AddListener(UpdateUsername);
        }
    }

    // ユーザー名更新
    void UpdateUsername(string newName)
    {
        PlayerPrefs.SetString(UsernameKey, newName);
    }

    // 音量UI更新
    void UpdateVolumeUI(float value)
    {
        volumeValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        AudioListener.volume = value;          // 実際の音量に反映
        PlayerPrefs.SetFloat("Volume", value); // 保存
    }

    // 設定パネルの開閉
    public void ToggleSetting()
    {
        // プレイヤー参照を取得
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        isOpened = !isOpened;
        settingPanel.SetActive(isOpened);
        // 設定画面中はプレイヤー操作を無効化
        if (isOpened)
        {
            playerController.isControlled(false);
        }
        else
        {
            playerController.isControlled(true);
        }
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }
    // 感度UI更新
    void UpdateSensitivityUI(float value)
    {
        sensitivityValueText.text = $"{value:F2}";
        PlayerPrefs.SetFloat("Sensitivity", value);
        // プレイヤーの視点感度に反映
        if (playerController != null)
        {
            playerController.SetLookSpeed(value);
        }
    }

    // 設定画面 → タイトル画面へ
    void OnBack()
    {
        settingPanel.SetActive(false);
        mainPanel.SetActive(true);
        // ゲームパッド用の初期選択
        EventSystem.current.SetSelectedGameObject(firstButtonOnTitlePanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
    }

    // ゲームオーバー画面へ遷移
    void OnGameOver()
    {
        AudioManager.instance.StopBGMWithFade(1.0f);
        SceneManager.LoadScene("GameOverScene");
    }
}