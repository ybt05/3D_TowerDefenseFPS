// MainScene Tower
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Tower : MonoBehaviour
{
    // タワー基礎ステータス
    private float maxHP = 100f;          // 最大HP
    private float currentHP;             // 現在HP
    [SerializeField] private Slider hpSlider; // HPバーUI
    private float damage = 0f;           // タワーの攻撃力
    private float attackRate = 5f;       // 攻撃間隔（秒）
    private bool CanAttack = false;      // 攻撃可能かどうか
    private int autoHealAmount = 0;      // 自動回復量
    private float autoHealInterval = 5f; // 自動回復の間隔（秒）

    // アップグレード関連
    private int edrUpgradeCost = 1000;      // EDRアップグレード費用
    private int firewallUpgradeCost = 1000; // Firewallアップグレード費用
    private int backupUpgradeCost = 100;    // Backupアップグレード費用
    private int accessControlUpgradeCost = 100; // AccessControlアップグレード費用
    private int antivirusUpgradeCost = 100; // Antivirusアップグレード費用
    private int edrLevel = 0;               // EDRレベル
    private int firewallLevel = 0;          // Firewallレベル
    private int backupLevel = 0;            // Backupレベル
    private int accessControlLevel = 0;     // AccessControlレベル
    private int antivirusLevel = 0;         // Antivirusレベル
    private float attackRange = 20f;     // 攻撃範囲
    private float attackTimer = 0f;      // 攻撃クールダウン
    private float slowRate = 0f;         // 敵の攻撃速度を遅くする
    private void Awake()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    // ステータス取得
    public float getMaxHP() => maxHP;
    public float getCurrentHP() => currentHP;
    public float getSlowRate() => slowRate;
    public (int edrCost, int firewallCost, int backupCost, int accessControlCost, int antivirusCost,
            float hp, float maxHp, float damage, int autoHeal,
            int edrLevel, int firewallLevel, int backupLevel, int accessControlLevel, int antivirusLevel)
    GetTowerStatus()
    {
        return (edrUpgradeCost, firewallUpgradeCost, backupUpgradeCost, accessControlUpgradeCost, antivirusUpgradeCost,
                currentHP, maxHP, damage, autoHealAmount,
                edrLevel, firewallLevel, backupLevel, accessControlLevel, antivirusLevel);
    }
    private void Update()
    {
        // 自動回復
        if (autoHealAmount > 0 && currentHP < maxHP)
        {
            autoHealInterval -= Time.deltaTime;
            if (autoHealInterval <= 0f)
            {
                currentHP += autoHealAmount;
                currentHP = Mathf.Clamp(currentHP, 0, maxHP);
                autoHealInterval = 5f;
                if (hpSlider != null)
                {
                    hpSlider.value = currentHP;
                }
            }
        }
        // 自動攻撃
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && CanAttack)
        {
            AttackNearestEnemy();
        }
    }
    // 最も近い敵を攻撃する
    private void AttackNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        float closestDist = Mathf.Infinity;
        Enemy target = null;
        // 範囲内の敵から最も近い敵を探す
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = hit.GetComponent<Enemy>();
                }
            }
        }
        // 攻撃
        if (target != null)
        {
            target.TakeDamage(damage, false, false);
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.towerAttackSound, 0.7f);
            attackTimer = attackRate;
        }
    }
    // 被ダメージ処理
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.towerDamagedSound, 0.5f);
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
        if (currentHP <= 0)
        {
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.towerDestroySound, 0.6f);
            GameOver();
        }
    }
    // アップグレード（EDR）
    public void UpgradeEDR()
    {
        if (ItemList.instance.getItemNum("コイン") >= edrUpgradeCost)
        {
            ItemList.instance.addItem("コイン", -edrUpgradeCost);
            damage += 10;
            autoHealAmount += 5;
            CanAttack = true;
            if (attackRate > 1f)
            {
                attackRate -= 1f;
            }
            edrUpgradeCost += 500;
            edrLevel++;
        }
    }
    // アップグレード（Firewall）
    public void UpgradeFirewall()
    {
        if (ItemList.instance.getItemNum("コイン") >= firewallUpgradeCost)
        {
            ItemList.instance.addItem("コイン", -firewallUpgradeCost);
            maxHP += 30;
            slowRate += 0.1f;
            firewallUpgradeCost += 500;
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }
            firewallLevel++;
        }
    }
    // アップグレード（Backup）
    public void UpgradeBackup()
    {
        if (ItemList.instance.getItemNum("コイン") >= backupUpgradeCost)
        {
            ItemList.instance.addItem("コイン", -backupUpgradeCost);
            autoHealAmount += 2;
            backupUpgradeCost += 100;
            backupLevel++;
        }
    }
    // アップグレード（Access Control）
    public void UpgradeAccessControl()
    {
        if (ItemList.instance.getItemNum("コイン") >= accessControlUpgradeCost)
        {
            ItemList.instance.addItem("コイン", -accessControlUpgradeCost);
            maxHP += 20;
            accessControlUpgradeCost += 100;
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }
            accessControlLevel++;
        }
    }
    // アップグレード（Antivirus）
    public void UpgradeAntivirus()
    {
        if (ItemList.instance.getItemNum("コイン") >= antivirusUpgradeCost)
        {
            ItemList.instance.addItem("コイン", -antivirusUpgradeCost);
            damage += 5;
            CanAttack = true;
            antivirusUpgradeCost += 100;
            antivirusLevel++;
        }
    }
    // ゲームオーバー処理
    public void GameOver()
    {
        AudioManager.instance.StopBGMWithFade(1.0f);
        SceneManager.LoadScene("GameOverScene");
    }
}