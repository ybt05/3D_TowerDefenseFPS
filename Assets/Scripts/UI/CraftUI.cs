// MainScene TowerCanvas/TowerPanel/CraftPanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CraftUI : MonoBehaviour
{
    public static CraftUI instance;

    [Header("Left Panel")]
    [SerializeField] private Transform itemListParent;      // クラフト可能アイテム一覧の親オブジェクト
    [SerializeField] private GameObject itemButtonPrefab;   // アイテムボタンのプレハブ

    [Header("Right Panel")]
    [SerializeField] private TextMeshProUGUI itemNameText;        // 選択中アイテム名
    [SerializeField] private TextMeshProUGUI itemInfoText;        // アイテムのステータス情報
    [SerializeField] private TextMeshProUGUI itemMessageText;     // アイテムの説明文
    [SerializeField] private TextMeshProUGUI requiredMaterialsText; // 必要素材一覧
    [SerializeField] private TextMeshProUGUI materialWarningText;   // 素材不足の警告
    [SerializeField] private Button craftButton;                   // クラフトボタン
    [SerializeField] private GameObject craftInfoPanel;            // 右側の詳細パネル

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;   // タワーパネル
    [SerializeField] private GameObject craftPanel;  // クラフトパネル

    [Header("Buttons")]
    [SerializeField] private Button backButton;              // 戻るボタン
    [SerializeField] private Button firstButtonOnMainPanel;  // 戻った時に選択されるボタン（ゲームパッド用）

    private string selectedItemName; // 現在選択中のアイテム名

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        PopulateItemList(); // 左側のクラフト可能アイテム一覧を生成
        craftButton.onClick.AddListener(CraftSelectedItem);
        craftInfoPanel.SetActive(false); // 初期状態では右パネル非表示
        backButton.onClick.AddListener(OnBack);
    }
    // クラフト可能アイテム一覧を生成
    void PopulateItemList()
    {
        foreach (string name in ItemList.instance.CraftableItemName)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, itemListParent);
            // ボタンのテキストにアイテム名を設定
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = name;
            // クリック時に OnItemSelected を呼ぶ
            string itemName = name;
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnItemSelected(itemName));
        }
    }

    // アイテムが選択されたときの処理
    void OnItemSelected(string itemName)
    {
        craftInfoPanel.SetActive(true);
        selectedItemName = itemName;
        itemNameText.text = itemName;
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        // アイテム情報（ステータス）を表示
        itemInfoText.text = ItemList.instance.getItemInfo(itemName);
        // アイテム説明文を表示
        itemMessageText.text = ItemList.instance.getItemMessage(itemName);
        // 必要素材を表示
        Dictionary<string, int> mat = ItemList.instance.GetItemMaterial(itemName);
        string matText = "";
        foreach (var pair in mat)
        {
            int playerHave = ItemList.instance.getItemNum(pair.Key);
            matText += $"{pair.Key}: {playerHave}/{pair.Value}\n";
        }
        requiredMaterialsText.text = matText;
        // 素材不足チェック
        string warning = "";
        foreach (var pair in mat)
        {
            int playerHave = ItemList.instance.getItemNum(pair.Key);
            if (playerHave < pair.Value)
            {
                warning += $"{pair.Key} が不足しています\n";
            }
        }
        materialWarningText.text = warning;
        materialWarningText.color = string.IsNullOrEmpty(warning) ? Color.green : Color.red;
    }

    // 銃かどうか判定（銃は1つしかクラフトできない）
    bool IsGun(string itemName)
    {
        return System.Array.Exists(ItemList.instance.GunName, gun => gun == itemName);
    }
    // クラフト処理
    void CraftSelectedItem()
    {
        if (string.IsNullOrEmpty(selectedItemName)) { return; }
        // 銃は1つしか持てないため、所持済みならクラフト不可
        if (IsGun(selectedItemName) && ItemList.instance.getItemNum(selectedItemName) > 0)
        {
            materialWarningText.text = "すでに所持しています";
            materialWarningText.color = Color.red;
            return;
        }
        // 必要素材を取得
        Dictionary<string, int> mat = ItemList.instance.GetItemMaterial(selectedItemName);
        // 素材チェック
        foreach (var pair in mat)
        {
            if (ItemList.instance.getItemNum(pair.Key) < pair.Value)
            {
                materialWarningText.text = $"{pair.Key} が不足しています";
                materialWarningText.color = Color.red;
                return;
            }
        }
        // アイテム生成
        ItemList.instance.createItem(selectedItemName);
        // 再表示して更新
        OnItemSelected(selectedItemName);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
    }

    // クラフト画面を閉じてタワーパネルへ戻る
    public void OnBack()
    {
        mainPanel.SetActive(true);
        craftPanel.SetActive(false);
        // ゲームパッド操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnMainPanel.gameObject);
    }
}