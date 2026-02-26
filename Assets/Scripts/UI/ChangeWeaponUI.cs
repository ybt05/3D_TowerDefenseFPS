// MainScene TowerCanvas/TowerPanel/ChangeWeaponPanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChangeWeaponUI : MonoBehaviour
{
    public static ChangeWeaponUI instance;

    [Header("武器UI")]
    [SerializeField] private GameObject weaponButtonPrefab; // 武器ボタンのプレハブ
    [SerializeField] private Transform buttonParent;        // ScrollView の Content
    [SerializeField] private TextMeshProUGUI currentWeaponText; // 現在装備中の武器名表示

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;         // タワーパネル
    [SerializeField] private GameObject changeWeaponPanel; // 武器変更パネル

    [Header("Buttons")]
    [SerializeField] private Button backButton;            // 戻るボタン
    [SerializeField] private Button firstButtonOnMainPanel; // 戻った時に選択されるボタン（ゲームパッド用）

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }
    private void Start()
    {
        // 戻るボタンのイベント登録
        backButton.onClick.AddListener(OnBack);
    }
    // 武器一覧ボタンを更新（パネルを開くたびに呼ばれる）
    public void UpdateWeaponButtons()
    {
        // 既存のボタンを全削除
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
        // 所持している武器だけボタンを生成
        foreach (string gunName in ItemList.instance.GunName)
        {
            int count = ItemList.instance.getGunCount(gunName);
            if (count > 0) // 所持している武器のみ表示
            {
                GameObject btnObj = Instantiate(weaponButtonPrefab, buttonParent);
                // ボタンのテキストに武器名を設定
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                btnText.text = gunName;
                string capturedGunName = gunName;
                // ボタンを押すと武器を切り替える
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    ItemList.instance.switchGun(capturedGunName); // 武器切り替え
                    AudioManager.instance.PlaySound(AudioManager.instance.audioClips.changeWeaponSound, 0.1f);
                    // 現在装備中の武器名を更新
                    currentWeaponText.text = ItemList.instance.getGunName();
                });
            }
        }
        // 現在の武器名を表示
        currentWeaponText.text = ItemList.instance.getGunName();
    }

    // 武器変更パネルを閉じてタワーパネルへ戻る
    public void OnBack()
    {
        mainPanel.SetActive(true);
        changeWeaponPanel.SetActive(false);
        // ゲームパッド操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }
}