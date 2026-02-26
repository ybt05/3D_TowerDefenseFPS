// TitleScene Canvas/GachaPanel
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GachaUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gachaPanel;   // ガチャ画面
    [SerializeField] private GameObject titlePanel;   // タイトル画面

    [Header("Buttons")]
    [SerializeField] private Button gachaButton;      // ガチャを回すボタン
    [SerializeField] private Button backButton;       // 戻るボタン
    [SerializeField] private Button firstButtonOnTitlePanel; // 戻った時に選択されるボタン

    [SerializeField] private TextMeshProUGUI coinText; // 所持コイン表示
    [SerializeField] private TextMeshProUGUI itemText; // お守りの所持数表示

    // ▼ ガチャ設定
    private const int GachaCost = 10000;       // ガチャ1回の必要コイン
    private const string CoinKey = "coins";    // コイン保存キー
    private const string CoinUpKey = "Charm_Coin";   // コイン増加お守り
    private const string TimeUpKey = "Charm_Time";   // 時間延長お守り
    private const string ScoreUpKey = "Charm_Score"; // スコア増加お守り
    private void Start()
    {
        // ボタンイベント登録
        gachaButton.onClick.AddListener(OnGacha);
        backButton.onClick.AddListener(OnBack);
        // 初期UI更新
        UpdateUI();
    }
    // UI更新（コイン・お守り数・ボタン状態）
    private void UpdateUI()
    {
        // Inspector 設定漏れチェック
        if (coinText == null) { Debug.LogError("coinText が設定されていません"); }
        if (itemText == null) { Debug.LogError("itemText が設定されていません"); }
        if (gachaButton == null) { Debug.LogError("gachaButton が設定されていません"); }
        // 所持コイン表示
        int coins = PlayerPrefs.GetInt(CoinKey, 0);
        coinText.text = $"ガチャコイン: {coins}";
        // お守りの所持数表示
        itemText.text =
            $"コインのお守り: {PlayerPrefs.GetInt(CoinUpKey, 0)}\n\n" +
            $"時間のお守り: {PlayerPrefs.GetInt(TimeUpKey, 0)}\n\n" +
            $"スコアのお守り: {PlayerPrefs.GetInt(ScoreUpKey, 0)}\n\n";
        // コイン不足ならガチャボタンを無効化
        gachaButton.interactable = coins >= GachaCost;
    }
    // ガチャ処理
    private void OnGacha()
    {
        int coins = PlayerPrefs.GetInt(CoinKey, 0);
        // コイン消費
        PlayerPrefs.SetInt(CoinKey, coins - GachaCost);
        // ランダムでお守りを1つ付与
        string[] charmKeys = { CoinUpKey, TimeUpKey, ScoreUpKey };
        string selectedKey = charmKeys[Random.Range(0, charmKeys.Length)];
        int current = PlayerPrefs.GetInt(selectedKey, 0);
        PlayerPrefs.SetInt(selectedKey, current + 1);
        PlayerPrefs.Save();
        // 効果音
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        // UI更新
        UpdateUI();
    }
    // タイトル画面へ戻る
    public void OnBack()
    {
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.cancelSound, 0.1f);
        gachaPanel.SetActive(false);
        titlePanel.SetActive(true);
        // ゲームパッド操作時に最初に選択されるボタンを設定
        EventSystem.current.SetSelectedGameObject(firstButtonOnTitlePanel.gameObject);
    }
}