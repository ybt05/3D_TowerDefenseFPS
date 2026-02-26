// MainScene TowerCanvas/TowerPanel/UpGradePanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerUpgradeUI : MonoBehaviour
{
    [Header("表示用UI")]
    [SerializeField] private TextMeshProUGUI coinText;                 // 所持コイン表示
    [SerializeField] private TextMeshProUGUI edrCostText;              // EDR強化のコスト表示
    [SerializeField] private TextMeshProUGUI firewallCostText;         // ファイアウォール強化のコスト表示
    [SerializeField] private TextMeshProUGUI backupCostText;           // バックアップ強化のコスト表示
    [SerializeField] private TextMeshProUGUI accessControlCostText;    // アクセス制御強化のコスト表示
    [SerializeField] private TextMeshProUGUI antivirusCostText;        // アンチウイルス強化のコスト表示

    [SerializeField] private TextMeshProUGUI edrLevelText;             // EDRレベル表示
    [SerializeField] private TextMeshProUGUI firewallLevelText;        // ファイアウォールレベル表示
    [SerializeField] private TextMeshProUGUI backupLevelText;          // バックアップレベル表示
    [SerializeField] private TextMeshProUGUI accessControlLevelText;   // アクセス制御レベル表示
    [SerializeField] private TextMeshProUGUI antivirusLevelText;       // アンチウイルスレベル表示

    [SerializeField] private TextMeshProUGUI hpText;                   // タワーHP表示
    [SerializeField] private TextMeshProUGUI damageText;               // タワー攻撃力表示
    [SerializeField] private TextMeshProUGUI autoHealText;             // 自然回復力表示

    [SerializeField] private Button edrButton;                         // EDR強化ボタン
    [SerializeField] private Button firewallButton;                    // ファイアウォール強化ボタン
    [SerializeField] private Button backupButton;                      // バックアップ強化ボタン
    [SerializeField] private Button accessControlButton;               // アクセス制御強化ボタン
    [SerializeField] private Button antivirusButton;                   // アンチウイルス強化ボタン

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;                     // タワーUIのメインパネル
    [SerializeField] private GameObject upGradePanel;                  // 強化パネル

    [Header("Buttons")]
    [SerializeField] private Button backButton;                        // 戻るボタン
    [SerializeField] private Button firstButtonOnMainPanel;            // 戻った時に選択されるボタン

    [Header("Tower")]
    [SerializeField] private Tower tower;                              // 強化対象のタワー

    private bool initialized = false;                                  // Startが完了したかどうか

    private void Start()
    {
        // 初期UI更新
        UpdateUI();
        // 戻るボタン
        backButton.onClick.AddListener(OnBack);
        // 各強化ボタンに処理を登録
        edrButton.onClick.AddListener(() =>
        {
            tower.UpgradeEDR();
            UpdateUI();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        });
        firewallButton.onClick.AddListener(() =>
        {
            tower.UpgradeFirewall();
            UpdateUI();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        });
        backupButton.onClick.AddListener(() =>
        {
            tower.UpgradeBackup();
            UpdateUI();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        });
        accessControlButton.onClick.AddListener(() =>
        {
            tower.UpgradeAccessControl();
            UpdateUI();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        });
        antivirusButton.onClick.AddListener(() =>
        {
            tower.UpgradeAntivirus();
            UpdateUI();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        });
        initialized = true;
    }

    private void OnEnable()
    {
        // Start前に呼ばれた場合はスキップ
        if (!initialized) { return; }
        // パネルが開いたタイミングで最新状態に更新
        UpdateUI();
    }

    // UI全体を最新のタワーステータスで更新
    private void UpdateUI()
    {
        if (tower == null) { return; }
        var status = tower.GetTowerStatus();
        // 所持金
        coinText.text = $"所持金: {ItemList.instance.getItemNum("コイン")}";
        // 強化コスト表示
        edrCostText.text = $"EDR: {status.edrCost}コイン (攻撃力++ 自然回復力++)";
        firewallCostText.text = $"ファイアウォール: {status.firewallCost}コイン (HP++)";
        backupCostText.text = $"バックアップ: {status.backupCost}コイン (自然回復力++)";
        accessControlCostText.text = $"アクセス制御: {status.accessControlCost}コイン (HP++)";
        antivirusCostText.text = $"アンチウイルスソフト: {status.antivirusCost}コイン (攻撃力++)";
        // タワーステータス表示
        hpText.text = $"HP: {status.hp}/{status.maxHp}";
        damageText.text = $"攻撃力: {status.damage}";
        autoHealText.text = $"自然回復力: {status.autoHeal}";
        // レベル表示
        edrLevelText.text = $"レベル{status.edrLevel}";
        firewallLevelText.text = $"レベル{status.firewallLevel}";
        backupLevelText.text = $"レベル{status.backupLevel}";
        accessControlLevelText.text = $"レベル{status.accessControlLevel}";
        antivirusLevelText.text = $"レベル{status.antivirusLevel}";
        // 所持金不足ならボタンを押せないようにする
        int coins = ItemList.instance.getItemNum("コイン");
        edrButton.interactable = coins >= status.edrCost;
        firewallButton.interactable = coins >= status.firewallCost;
        backupButton.interactable = coins >= status.backupCost;
        accessControlButton.interactable = coins >= status.accessControlCost;
        antivirusButton.interactable = coins >= status.antivirusCost;
    }

    // メインパネルへ戻る
    public void OnBack()
    {
        mainPanel.SetActive(true);
        upGradePanel.SetActive(false);
        // ゲームパッド用の初期選択
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }
}