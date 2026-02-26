// MainScene Canvas/HPBar
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarController : MonoBehaviour
{
    [Header("プレイヤー")]
    [SerializeField] private Slider playerHPBar;          // プレイヤーHPバー
    [SerializeField] private PlayerHealth playerHealth;   // プレイヤーのHP管理クラス
    [SerializeField] private TextMeshProUGUI playerHPText; // HP数値表示

    [Header("タワー")]
    [SerializeField] private Slider towerHPBar;           // タワーHPバー
    [SerializeField] private Tower tower;                 // タワーのHP管理クラス

    private void Start()
    {
        // 初期最大値を設定
        if (playerHPBar != null)
        {
            playerHPBar.maxValue = playerHealth.getMaxHP();
        }
        if (towerHPBar != null)
        {
            towerHPBar.maxValue = tower.getMaxHP();
        }
    }
    private void Update()
    {
        // プレイヤーHPバー更新
        if (playerHPBar != null)
        {
            // 最大HPが変動する可能性があるため毎フレーム更新
            playerHPBar.maxValue = playerHealth.getMaxHP();
            playerHPBar.value = playerHealth.getCurrentHP();
        }
        // プレイヤーHPの数値表示
        if (playerHPText != null)
        {
            playerHPText.text =
                $"{Mathf.FloorToInt(playerHealth.getCurrentHP())} / {Mathf.FloorToInt(playerHealth.getMaxHP())}";
        }
        // タワーHPバー更新
        if (towerHPBar != null)
        {
            towerHPBar.maxValue = tower.getMaxHP();
            towerHPBar.value = tower.getCurrentHP();
        }
    }
}