// MainScene TowerCanvas/TowerPanel
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject towerUI;          // タワーUI全体
    [SerializeField] private GameObject mainPanel;        // メインメニュー
    [SerializeField] private GameObject craftPanel;       // クラフトパネル
    [SerializeField] private GameObject upgradePanel;     // 強化パネル
    [SerializeField] private GameObject changeWeaponPanel;// 武器変更パネル
    [SerializeField] private GameObject quizPanel;        // クイズパネル
    [SerializeField] private GameObject explanationPanel; // クイズ解説パネル
    [SerializeField] private GameObject typingPanel;      // タイピングパネル
    [SerializeField] private CanvasGroup towerCanvasGroup;// UIの操作可否制御

    [Header("Buttons")]
    [SerializeField] private Button upGradeButton;        // 強化ボタン
    [SerializeField] private Button craftButton;          // クラフトボタン
    [SerializeField] private Button changeWeaponButton;   // 武器変更ボタン
    [SerializeField] private Button closeButton;          // UIを閉じるボタン
    [SerializeField] private Button quizButton;           // クイズボタン

    [SerializeField] private Button firstButtonOnUpGradePanel;     // 強化パネルの初期選択
    [SerializeField] private Button firstButtonOnCraftPanel;       // クラフトパネルの初期選択
    [SerializeField] private Button firstButtonOnChangeWeaponPanel;// 武器変更パネルの初期選択
    [SerializeField] private Button firstButtonOnQuizPanel;        // クイズパネルの初期選択
    [SerializeField] private Button firstButtonOnTypingPanel;      // タイピングパネルの初期選択

    [Header("Player")]
    [SerializeField] private Transform player;            // プレイヤー参照
    private PlayerController playerController;            // プレイヤー操作制御

    private bool isUIOpen = false;                        // UIが開いているかどうか
    public static TowerUIManager instance;

    private void Awake()
    {
        // シングルトン化
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private IEnumerator Start()
    {
        // プレイヤーが生成されるまで待機
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        // 全パネルを非表示で初期化
        towerUI.SetActive(false);
        mainPanel.SetActive(false);
        craftPanel.SetActive(false);
        upgradePanel.SetActive(false);
        changeWeaponPanel.SetActive(false);
        explanationPanel.SetActive(false);
        quizPanel.SetActive(false);
        typingPanel.SetActive(false);
        // ボタンイベント登録
        upGradeButton.onClick.AddListener(ShowUpgradePanel);
        craftButton.onClick.AddListener(ShowCraftPanel);
        changeWeaponButton.onClick.AddListener(ShowChangeWeaponPanel);
        closeButton.onClick.AddListener(CloseUI);
        quizButton.onClick.AddListener(ShowQuizPanel);
    }
    // タワーUIの開閉
    public void ToggleUI()
    {
        isUIOpen = !isUIOpen;
        towerUI.SetActive(isUIOpen);
        if (isUIOpen)
        {
            // UI開いている間はプレイヤー操作を無効化
            playerController.isControlled(false);
            ShowMainPanel();
        }
        else
        {
            playerController.isControlled(true);
        }
    }

    // UIの操作可否を切り替える
    public void SetInteractable(bool value)
    {
        towerCanvasGroup.interactable = value;
        towerCanvasGroup.blocksRaycasts = value;
    }
    // メインパネル表示
    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        craftPanel.SetActive(false);
        upgradePanel.SetActive(false);
        changeWeaponPanel.SetActive(false);
        quizPanel.SetActive(false);
        explanationPanel.SetActive(false);
        typingPanel.SetActive(false);
        // ゲームパッド用の初期選択
        EventSystem.current.SetSelectedGameObject(upGradeButton.gameObject);
        // クイズ解説状態を解除
        EnemySpawner.instance.QuizExplanation(false);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // クラフトパネル表示
    public void ShowCraftPanel()
    {
        mainPanel.SetActive(false);
        craftPanel.SetActive(true);
        upgradePanel.SetActive(false);
        changeWeaponPanel.SetActive(false);
        quizPanel.SetActive(false);
        explanationPanel.SetActive(false);
        typingPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstButtonOnCraftPanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // 強化パネル表示
    public void ShowUpgradePanel()
    {
        mainPanel.SetActive(false);
        craftPanel.SetActive(false);
        upgradePanel.SetActive(true);
        changeWeaponPanel.SetActive(false);
        quizPanel.SetActive(false);
        explanationPanel.SetActive(false);
        typingPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstButtonOnUpGradePanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // クイズパネル表示
    public void ShowQuizPanel()
    {
        mainPanel.SetActive(false);
        craftPanel.SetActive(false);
        upgradePanel.SetActive(false);
        changeWeaponPanel.SetActive(false);
        // MainManager が無い or ジャンルが typing の場合 → タイピングパネル
        if (MainManager.instance == null || MainManager.instance.getGenre() == "typing")
        {
            typingPanel.SetActive(true);
            quizPanel.SetActive(false);
            explanationPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(firstButtonOnTypingPanel.gameObject);
        }
        else
        {
            // literacy の場合 → クイズパネル
            typingPanel.SetActive(false);
            quizPanel.SetActive(true);
            explanationPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(firstButtonOnQuizPanel.gameObject);
        }
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // 武器変更パネル表示
    public void ShowChangeWeaponPanel()
    {
        mainPanel.SetActive(false);
        craftPanel.SetActive(false);
        upgradePanel.SetActive(false);
        changeWeaponPanel.SetActive(true);
        quizPanel.SetActive(false);
        explanationPanel.SetActive(false);
        typingPanel.SetActive(false);
        // 武器ボタンを更新
        ChangeWeaponUI.instance.UpdateWeaponButtons();
        EventSystem.current.SetSelectedGameObject(firstButtonOnChangeWeaponPanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // UIを閉じる
    public void CloseUI()
    {
        if (isUIOpen)
        {
            towerUI.SetActive(false);
            isUIOpen = false;
            playerController.towerClose();
            playerController.isControlled(true);
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
        }
    }
}