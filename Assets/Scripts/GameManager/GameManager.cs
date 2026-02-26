//MainScene Manager/GameManagerObject
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float score = 0;            // 現在のスコア
    private int scorePerEnemy = 10;     // 敵1体あたりの基本スコア
    [SerializeField] private GameObject settingPanel; // 設定パネル
    private float lastKillTime = -999f; // 最後に敵を倒した時間
    private float comboMultiplier = 1;  // 現在のコンボ倍率
    private const float maxMultiplier = 10; // コンボ倍率の上限
    private const float comboWindow = 3f;   // コンボ継続可能時間（秒）

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        settingPanel.SetActive(false);
        // プレイヤーの感度を設定
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.1f); 
        if (PlayerController.instance != null)
        {
            PlayerController.instance.SetLookSpeed(savedSensitivity);
        }
    }
    // 敵撃破時の処理
    public void EnemyDefeated()
    {
        float currentTime = Time.time;
        // コンボ判定 前回から5秒以内なら倍率アップ
        if (currentTime - lastKillTime <= comboWindow)
        {
            comboMultiplier = Mathf.Min(comboMultiplier + 0.1f, maxMultiplier);
            comboMultiplier = Mathf.Round(comboMultiplier * 10f) / 10f;
        }
        else
        {
            comboMultiplier = 1; // リセット
        }
        lastKillTime = currentTime;
        // スコア加算
        float comboScore = scorePerEnemy * comboMultiplier;
        AddScore(comboScore);
        // UI更新(スコアと倍率を表示)
        if (UIController.instance != null)
        {
            UIController.instance.ShowScore(score, comboMultiplier);
        }
        if (MainManager.instance != null)
        {
            MainManager.instance.changeScore(score);
        }
    }
    private void AddScore(float amount)
    {
        score += amount;
    }
}
