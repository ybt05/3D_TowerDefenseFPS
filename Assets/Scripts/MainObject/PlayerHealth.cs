// MainScene Player
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // プレイヤー基礎ステータス
    private float maxHealth = 100f;          // 最大HP
    private float currentHealth;             // 現在HP

    private float movementSpeedParse = 1f;   // 移動速度倍率
    private float HPParse = 1f;              // HP倍率

    private float power = 1f;                // 基礎攻撃力
    private float powerParse = 1f;           // 攻撃力倍率

    private float defense = 0f;              // 防御力
    private float criticalRate = 0f;         // 会心率
    private float criticalDamage = 1f;       // 会心ダメージ倍率

    private float healthRegeneRate = 0f;     // 自然回復量（3秒ごと）
    private float damageCoolDown = 0f;       // ダメージ後の無敵時間
    private float respawnTime = 3f;          // 死亡後の復活時間
    private float invincibleTime = 2f;       // 復活後の無敵時間

    private Vector3 spawnPoint;              // 復活地点
    private bool isDead = false;             // 死亡状態
    private bool isInvincible = false;       // 無敵状態

    [SerializeField] private UnityEvent<float, float> OnHealthChanged; // HP更新イベント

    public static PlayerHealth instance;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }
    private void Start()
    {
        ApplyAccessoryStats();        // アクセサリー効果を反映
        maxHealth = maxHealth * HPParse;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        spawnPoint = transform.position; // 初期位置を復活地点に設定

        StartCoroutine(AutoHeal());      // 自然回復開始
    }
    // ステータス取得系
    public float getMovementSpeedParse() => movementSpeedParse;
    public float getMaxHP() => maxHealth;
    public float getCurrentHP() => currentHealth;
    public float getDefense() => defense;
    public bool getIsDead() => isDead;

    // プレイヤーの攻撃力計算
    public (float damage, bool isCritical) GetDamage()
    {
        float baseDamage = (ItemList.instance.getGunDamage() + power) * powerParse;
        bool isCritical = Random.value < criticalRate;
        float damage = isCritical ? baseDamage * criticalDamage : baseDamage;
        return (damage, isCritical);
    }

    // UI用のまとめステータス
    public (float maxHealth, float power, float defense, float totalPower, float criticalRate, float criticalDamage,
            float healthRegeneRate, float damageCoolDown, float respawnTime, float speed) getStatus()
    {
        float totalPower = (ItemList.instance.getGunDamage() + power) * powerParse;
        return (maxHealth, power, defense, totalPower, criticalRate, criticalDamage,
                healthRegeneRate, damageCoolDown, respawnTime, PlayerController.instance.getMovementSpeed());
    }
    // アクセサリー効果の反映
    private void ApplyAccessoryStats()
    {
        if (MainManager.instance == null || MainManager.instance.equippedAccessories == null) { return; }
        foreach (var acc in MainManager.instance.equippedAccessories)
        {
            if (acc == null) { continue; }
            // メインステータス
            ApplyStat(acc.mainStat.statName, acc.mainStat.value);
            // サブステータス
            foreach (var sub in acc.subStats)
            {
                ApplyStat(sub.statName, sub.value);
            }
        }
    }

    // ステータス名に応じて値を反映
    private void ApplyStat(string statName, float value)
    {
        switch (statName)
        {
            case "攻撃力": powerParse += value * 0.01f; break;
            case "防御力": defense += value; break;
            case "HP": HPParse += value * 0.01f; break;
            case "移動速度": movementSpeedParse += value * 0.01f; break;
            case "自然回復": healthRegeneRate += value; break;
            case "無敵時間": damageCoolDown += value; break;
            case "復活時間短縮": respawnTime = Mathf.Max(0.5f, respawnTime - value); break;
            case "会心率": criticalRate += value * 0.01f; break;
            case "会心ダメージ": criticalDamage += value * 0.01f; break;
        }
    }
    // 自然回復（3秒ごと）
    private IEnumerator AutoHeal()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (!isDead && currentHealth < maxHealth)
            {
                Heal(healthRegeneRate);
                OnHealthChanged.Invoke(currentHealth, maxHealth);
            }
        }
    }
    // ダメージ処理
    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) { return; }
        float finalDamage = Mathf.Max(damage - defense, 1f);
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged.Invoke(currentHealth, maxHealth);
        // 無敵時間開始
        StartCoroutine(DamageCoolDown());
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.playerDamagedSound, 0.7f);
        if (currentHealth <= 0)
        {
            Die();
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.playerDeathSound, 0.7f);
        }
    }
    // ダメージ後の無敵時間
    private IEnumerator DamageCoolDown()
    {
        isInvincible = true;
        yield return new WaitForSeconds(damageCoolDown);
        isInvincible = false;
    }
    // 死亡処理
    private void Die()
    {
        if (isDead) { return; }
        isDead = true;
        // プレイヤーを非表示 & 操作不能に
        GetComponent<CharacterController>().enabled = false;
        SetRenderersActive(false);
        // 復活処理へ
        StartCoroutine(Respawn());
    }
    // 回復処理
    public void Heal(float amount)
    {
        if (isDead) { return; }
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    // 復活処理
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        // HP全回復
        currentHealth = maxHealth;
        OnHealthChanged.Invoke(currentHealth, maxHealth);
        // 復活地点へ移動
        transform.position = spawnPoint;
        // 無敵状態で復活
        isInvincible = true;
        // 表示と操作を戻す
        SetRenderersActive(true);
        GetComponent<CharacterController>().enabled = true;
        isDead = false;
        // 復活後の無敵時間
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }
    // プレイヤーの見た目ON/OFF
    private void SetRenderersActive(bool active)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = active;
        }
    }
    // クイズ正解時の強化処理
    public void ReinforcementPlayer()
    {
        power += 2f;
        defense += 0.5f;
        maxHealth += 2f;
        currentHealth += 2f;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        AudioManager.instance.PlaySound(AudioManager.instance.audioClips.levelUpSound, 0.1f);
    }
}