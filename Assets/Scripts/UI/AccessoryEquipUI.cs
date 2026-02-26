// TitleScene Canvas/AccessoryPanel
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AccessoryEquipUI : MonoBehaviour
{
    public static AccessoryEquipUI instance;

    [Header("Left Panel (装備中)")]
    [SerializeField] private Transform equippedListParent;   // 装備スロット一覧（ScrollView Content）
    [SerializeField] private GameObject equippedSlotPrefab;  // 装備スロットのプレハブ

    [Header("Right Panel (所持一覧)")]
    [SerializeField] private Transform accessoryListParent;  // 所持アクセサリー一覧（ScrollView Content）
    [SerializeField] private GameObject accessoryItemPrefab; // 所持アクセサリー表示用プレハブ

    [Header("合計ステータス表示")]
    [SerializeField] private TextMeshProUGUI totalStatText;  // 装備中アクセサリーの合計ステータス表示

    [Header("警告表示")]
    [SerializeField] private TextMeshProUGUI warningText;    // 装備不可・削除警告などのメッセージ

    [Header("UI Panels")]
    [SerializeField] private GameObject difficultyPanel;     // 難易度選択パネル（戻る時に使用）
    [SerializeField] private GameObject mainPanel;           // タイトルメインパネル
    [SerializeField] private GameObject equipPanel;          // アクセサリー装備パネル
    [SerializeField] private GameObject genrePanel;          // ジャンル選択パネル（戻る時に使用）

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;           // 装備確定ボタン
    [SerializeField] private Button backButton;              // 戻るボタン
    [SerializeField] private Button deleteButton;            // 削除モード切替ボタン
    [SerializeField] private Button firstButtonOnTitlePanel; // タイトル画面に戻った時に選択されるボタン
    [SerializeField] private Button firstButtonOnGenrePanel; // ジャンル画面に戻った時に選択されるボタン
    [SerializeField] private Button firstButtonOnAccessoryPanel; // 装備画面を開いた時に選択されるボタン

    private int selectedSlotIndex = -1;   // 現在選択中の装備スロット
    private AccessoryInventory inventory; // 所持アクセサリー & 装備情報
    private bool isDeleteMode = false;    // 削除モード中かどうか

    private void Start()
    {
        // 暗号化されたアクセサリー情報をロード
        inventory = FindFirstObjectByType<AccessoryManager>().LoadAccessoriesEncrypted();
        // 所持アクセサリーが空の場合は初期化（初回プレイなど）
        if (inventory.ownedAccessories == null || inventory.ownedAccessories.Count == 0)
        {
            InitializeInventory();
        }
        // 装備スロット（5枠）を生成し、クリックイベントを登録
        for (int i = 0; i < 5; i++)
        {
            GameObject slot = Instantiate(equippedSlotPrefab, equippedListParent);
            int index = i;
            slot.GetComponent<Button>().onClick.AddListener(() => OnSlotClicked(index));
        }
        // 装備中アクセサリー表示更新
        UpdateEquippedDisplay();
        // 所持アクセサリー一覧を生成
        PopulateAccessoryList();
        // ボタンイベント登録
        confirmButton.onClick.AddListener(OnConfirm);
        backButton.onClick.AddListener(OnBack);
        deleteButton.onClick.AddListener(OnDeleteModeToggle);
    }

    // 警告メッセージを2秒だけ表示
    void ShowWarning(string message)
    {
        warningText.text = $"<color=#ff0000>{message}</color>";
        CancelInvoke(nameof(ClearWarning));
        Invoke(nameof(ClearWarning), 2f);
    }
    void ClearWarning()
    {
        warningText.text = "";
    }
    // 削除モードのON/OFF
    public void OnDeleteModeToggle()
    {
        isDeleteMode = !isDeleteMode;
        ShowWarning(isDeleteMode ? "アクセサリーをクリックで削除" : "");
        PopulateAccessoryList(); // 色変更のため再描画
    }
    // 装備中アクセサリーの合計ステータスを集計して表示
    void UpdateTotalStatsDisplay()
    {
        Dictionary<string, float> statTotals = new Dictionary<string, float>();
        // 装備中アクセサリーを集計
        foreach (var acc in inventory.equippedAccessories)
        {
            if (acc == null) { continue; }
            // メインステータス
            if (!statTotals.ContainsKey(acc.mainStat.statName))
            {
                statTotals[acc.mainStat.statName] = 0;
            }
            statTotals[acc.mainStat.statName] += acc.mainStat.value;
            // サブステータス
            foreach (var sub in acc.subStats)
            {
                if (!statTotals.ContainsKey(sub.statName))
                {
                    statTotals[sub.statName] = 0;
                }
                statTotals[sub.statName] += sub.value;
            }
        }
        // 表示するステータス一覧
        string[] allStatNames =
        {
            "攻撃力", "防御力", "HP", "移動速度", "自然回復",
            "無敵時間", "復活時間短縮", "会心率", "会心ダメージ"
        };
        string display = "";
        int i = 0;
        foreach (var stat in allStatNames)
        {
            float value = statTotals.ContainsKey(stat) ? statTotals[stat] : 0;
            // %表記が必要なステータス
            if (stat == "攻撃力" || stat == "HP" || stat == "移動速度" || stat == "会心率" || stat == "会心ダメージ")
            {
                display += $"{stat}:{value:F2}% ";
            }
            else
            {
                display += $"{stat}:{value:F2} ";
            }
            if (i % 3 == 2) { display += "\n"; }
            i++;
        }
        totalStatText.text = display.TrimEnd();
    }
    // 初回プレイなどでアクセサリーが存在しない場合の初期化
    void InitializeInventory()
    {
        inventory.ownedAccessories = FindFirstObjectByType<AccessoryManager>().LoadOwnedAccessories();
        inventory.equippedAccessories = new List<AccessoryData>(new AccessoryData[5]);
    }
    // 装備スロット表示更新
    void UpdateEquippedDisplay()
    {
        for (int i = 0; i < equippedListParent.childCount; i++)
        {
            TextMeshProUGUI text = equippedListParent.GetChild(i)
                .Find("AccessoryInfoText").GetComponent<TextMeshProUGUI>();
            var acc = inventory.equippedAccessories[i];
            if (acc != null && !string.IsNullOrEmpty(acc.accessoryName) && acc.mainStat != null)
            {
                text.text = GetAccessoryDisplayText(acc);
            }
            else
            {
                text.text = "<color=#888888>装備していません</color>";
            }
        }
        UpdateTotalStatsDisplay();
    }
    // アクセサリーの表示テキスト生成
    string GetAccessoryDisplayText(AccessoryData acc)
    {
        if (acc == null || acc.mainStat == null)
        {
            return "<color=#888888>アクセサリーがありません</color>";
        }
        string nameLine =
            $"<color=#0000FF>{acc.accessoryName}</color> " +
            $"<color=#FF0000>{acc.mainStat.statName}:{acc.mainStat.value}</color>\n";
        string mainLine = "";
        int i = 0;
        foreach (var sub in acc.subStats)
        {
            mainLine += $" <color=#006400>{sub.statName}:{sub.value}</color>";
            if (i % 3 == 2) { mainLine += "\n"; }
            i++;
        }
        return nameLine + mainLine;
    }

    // 所持アクセサリー一覧生成
    void PopulateAccessoryList()
    {
        // 既存のUIをクリア
        foreach (Transform child in accessoryListParent)
        {
            Destroy(child.gameObject);
        }
        // 並び替え（ステータス名 → 値）
        var sorted = new List<AccessoryData>(inventory.ownedAccessories);
        sorted.Sort((a, b) =>
        {
            if (a == null || a.mainStat == null) { return 1; }
            if (b == null || b.mainStat == null) { return -1; }
            int nameCompare = string.Compare(a.mainStat.statName, b.mainStat.statName);
            if (nameCompare != 0) { return nameCompare; }
            return b.mainStat.value.CompareTo(a.mainStat.value);
        });
        // UI生成
        foreach (var acc in sorted)
        {
            GameObject btn = Instantiate(accessoryItemPrefab, accessoryListParent);
            btn.transform.Find("AccessoryInfoText")
                .GetComponent<TextMeshProUGUI>().text = GetAccessoryDisplayText(acc);
            // 削除モードなら赤っぽく
            var image = btn.GetComponent<Image>();
            if (image != null)
            {
                image.color = isDeleteMode ? new Color(1f, 0.6f, 0.6f) : Color.white;
            }
            btn.GetComponent<Button>().onClick.AddListener(() => OnAccessorySelected(acc));
        }
    }
    // 装備スロットがクリックされたとき
    void OnSlotClicked(int index)
    {
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        selectedSlotIndex = index;
        UpdateSlotHighlight();
    }
    // 装備スロットのハイライト更新
    void UpdateSlotHighlight()
    {
        for (int i = 0; i < equippedListParent.childCount; i++)
        {
            var slot = equippedListParent.GetChild(i);
            var image = slot.GetComponent<Image>();
            if (image != null)
            {
                image.color = (i == selectedSlotIndex) ? new Color(1f, 1f, 0.6f) : Color.white;
            }
        }
    }

    // 他スロットに同じアクセサリーが装備されていないか確認
    bool IsAlreadyEquippedElsewhere(string acName, int excludeIndex)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i == excludeIndex) { continue; }
            var acc = inventory.equippedAccessories[i];
            if (acc != null && acc.accessoryName == acName) { return true; }
        }
        return false;
    }

    // アクセサリーが選択されたとき（装備 or 削除）
    void OnAccessorySelected(AccessoryData selected)
    {
        // 削除モード
        if (isDeleteMode)
        {
            // 装備中は削除不可
            if (inventory.equippedAccessories.Contains(selected))
            {
                ShowWarning($"{selected.accessoryName} は装備中です");
                return;
            }
            // 所持リストから削除
            bool removed = inventory.ownedAccessories.Remove(selected);
            if (removed)
            {
                ShowWarning($"{selected.accessoryName} を削除しました");
                var manager = FindFirstObjectByType<AccessoryManager>();
                manager.SaveAccessories(inventory);
                PopulateAccessoryList();
                EventSystem.current.SetSelectedGameObject(firstButtonOnAccessoryPanel.gameObject);
            }
            else
            {
                ShowWarning("削除できませんでした");
            }
            return;
        }
        // 装備モード
        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventory.equippedAccessories.Count) { return; }
        if (IsAlreadyEquippedElsewhere(selected.accessoryName, selectedSlotIndex))
        {
            ShowWarning($"{selected.accessoryName}はすでに装備されています");
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
            return;
        }
        // 装備処理
        inventory.equippedAccessories[selectedSlotIndex] = selected;
        UpdateEquippedDisplay();
    }

    // 現在装備中のアクセサリー一覧を返す
    public List<AccessoryData> GetEquippedAccessories()
    {
        return inventory.equippedAccessories;
    }
    // 装備確定 → ジャンル選択画面へ
    public void OnConfirm()
    {
        if (MainManager.instance == null)
        {
            Debug.Log("AccessoryUI.cs MainManager が見つかりません");
            return;
        }
        MainManager.instance.equippedAccessories = new List<AccessoryData>(inventory.equippedAccessories);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        equipPanel.SetActive(false);
        genrePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButtonOnGenrePanel.gameObject);
    }

    // タイトルメインパネルへ戻る
    void OnBack()
    {
        equipPanel.SetActive(false);
        mainPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButtonOnTitlePanel.gameObject);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
    }
}