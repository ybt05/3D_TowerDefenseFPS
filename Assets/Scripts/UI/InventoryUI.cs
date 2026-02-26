// MainScene Canvas/InventoryPanel
using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;
    [SerializeField] private GameObject itemTextPrefab;     // アイテム名＋所持数を表示するプレハブ
    [SerializeField] private Transform contentParent;       // アイテム一覧の親（ScrollView Content）
    [SerializeField] private Transform InventoryPanel;      // インベントリ全体パネル
    [SerializeField] private TMPro.TextMeshProUGUI statusText; // プレイヤーステータス表示
    private List<GameObject> itemTexts = new List<GameObject>(); // 生成したテキストの管理用

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        // ゲーム開始時はインベントリを非表示
        if (InventoryPanel != null)
        {
            InventoryPanel.gameObject.SetActive(false);
        }
    }
    // インベントリの開閉
    public void ToggleInventory()
    {
        if (InventoryPanel == null)
        {
            Debug.Log("InventoryUI.cs InventoryPanelが存在しません");
            return;
        }
        bool isActive = InventoryPanel.gameObject.activeSelf;
        InventoryPanel.gameObject.SetActive(!isActive);
        // インベントリ表示中はプレイヤー操作を無効化
        if (!isActive)
        {
            PlayerController.instance.isControlled(false);
        }
        else
        {
            PlayerController.instance.isControlled(true);
        }
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        // 開いたときだけ内容を更新
        if (!isActive)
        {
            UpdateInventory();
        }
    }
    // インベントリ内容を更新（アイテム一覧＋ステータス）
    private void UpdateInventory()
    {
        // 既存のテキストを削除
        foreach (var t in itemTexts)
        {
            Destroy(t);
        }
        itemTexts.Clear();
        // アイテム一覧を追加（所持数が0のものは表示しない）
        AddItemText("コイン", ItemList.instance.getItemNum("コイン"));
        AddItemText("木材", ItemList.instance.getItemNum("木材"));
        AddItemText("鉄", ItemList.instance.getItemNum("鉄"));
        AddItemText("金", ItemList.instance.getItemNum("金"));
        AddItemText("火薬", ItemList.instance.getItemNum("火薬"));
        AddItemText("ポテト", ItemList.instance.getItemNum("ポテト"));
        AddItemText("ニンジン", ItemList.instance.getItemNum("ニンジン"));
        AddItemText("ピストル", ItemList.instance.getItemNum("ピストル"));
        AddItemText("ショットガン", ItemList.instance.getItemNum("ショットガン"));
        AddItemText("ハンドガン", ItemList.instance.getItemNum("ハンドガン"));
        AddItemText("サブマシンガン", ItemList.instance.getItemNum("サブマシンガン"));
        AddItemText("アサルトライフル", ItemList.instance.getItemNum("アサルトライフル"));
        AddItemText("スナイパー", ItemList.instance.getItemNum("スナイパー"));
        AddItemText("ライトマシンガン", ItemList.instance.getItemNum("ライトマシンガン"));
        AddItemText("小石", ItemList.instance.getItemNum("小石"));
        AddItemText("鉄の弾", ItemList.instance.getItemNum("鉄の弾"));
        AddItemText("炸裂弾", ItemList.instance.getItemNum("炸裂弾"));
        AddItemText("ベイクドポテト", ItemList.instance.getItemNum("ベイクドポテト"));
        AddItemText("シチュー", ItemList.instance.getItemNum("シチュー"));
        AddItemText("タレット", ItemList.instance.getItemNum("タレット"));
        // プレイヤーステータス表示更新
        UpdateStatusText();
    }
    // アイテム1つ分のテキストを生成
    private void AddItemText(string itemName, int count)
    {
        if (count <= 0) { return; } // 所持数0なら表示しない
        GameObject newText = Instantiate(itemTextPrefab, contentParent);
        var tmp = newText.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = $"{itemName} × {count}";
        }
        else
        {
            Debug.Log("InventoryUI.cs itemTextPrefabにTextMeshProUGUIがアタッチされていません");
        }
        itemTexts.Add(newText);
    }
    // プレイヤーの総合ステータスを表示
    private void UpdateStatusText()
    {
        var status = PlayerHealth.instance.getStatus();
        statusText.text =
            $"最大HP：{status.maxHealth}\n" +
            $"基礎攻撃力：{status.power}\n" +
            $"攻撃力：{status.totalPower:F2}\n" +
            $"防御力：{status.defense:F2}\n" +
            $"会心率：{status.criticalRate * 100:F1}%\n" +
            $"会心ダメージ：{status.criticalDamage * 100:F1}%\n" +
            $"自然回復力：{status.healthRegeneRate:F2}\n" +
            $"無敵時間：{status.damageCoolDown:F2}秒\n" +
            $"復活時間：{status.respawnTime:F2}秒\n" +
            $"移動速度：{status.speed:F2}\n";
    }
}