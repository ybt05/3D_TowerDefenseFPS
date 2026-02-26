// Prefabs/MonsterPrefab
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Enemy : MonoBehaviour
{
    // 敵の行動タイプ
    public enum EnemyType
    {
        TowerOnly,   // タワーだけを狙う
        PlayerOnly,  // プレイヤーだけを狙う
        Hybrid,      // 状況でターゲットを切り替える
        CanShoot     // プレイヤーを狙い、射撃も行う
    }
    private EnemyType enemyType = EnemyType.Hybrid;
    // ステータス
    private float maxHP = 50f;        // 最大HP
    private float currentHP;          // 現在HP
    private float damage = 10f;       // 与えるダメージ
    private float attackRate = 1f;    // 攻撃間隔（秒）
    private float attackTimer = 0f;   // 攻撃クールダウン用タイマー
    private bool isDead = false;      // 死亡フラグ

    [Header("UI")]
    [SerializeField] private GameObject damageTextPrefab; // ダメージ表示テキストのプレハブ

    [Header("移動設定")]
    [SerializeField] private float stoppingDistance = 15f; // 攻撃開始距離（NavMeshAgentの停止距離）
    private float speed = 0f;                               // 現在の移動速度（アニメーション用）
    private Transform targetTower;                          // タワーのTransform
    private Transform player;                               // プレイヤーのTransform
    private NavMeshAgent agent;                             // NavMeshAgent
    private int waveNumber = 1;                             // どのWaveで出現した敵か

    [Header("射撃設定")]
    [SerializeField] private GameObject bulletPrefab;       // 敵が撃つ弾のプレハブ
    [SerializeField] private Transform shootPoint;          // 弾の発射位置
    [SerializeField] private float bulletSpeed = 100f;      // 弾速
    [SerializeField] private float bulletLifetime = 5f;     // 弾の寿命
    private bool canShoot = false;                          // 射撃可能な敵かどうか
    private Animator _animator;                             // アニメーション制御
    private void Awake()
    {
        currentHP = maxHP;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        // タワーとプレイヤーを取得
        GameObject towerObj = GameObject.FindGameObjectWithTag("Tower");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (towerObj != null) { targetTower = towerObj.transform; }
        if (playerObj != null) { player = playerObj.transform; }
        // 初期ターゲット設定
        switch (enemyType)
        {
            case EnemyType.TowerOnly:
                if (targetTower != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(targetTower.position);
                }
                break;
            case EnemyType.PlayerOnly:
            case EnemyType.CanShoot:
                if (player != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);
                }
                break;
            case EnemyType.Hybrid:
                if (targetTower != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(targetTower.position);
                }
                break;
        }
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh) { return; }
        attackTimer += Time.deltaTime;
        // タイプ別の行動
        switch (enemyType)
        {
            case EnemyType.TowerOnly:
                AttackTowerOnly();
                break;
            case EnemyType.PlayerOnly:
                AttackPlayerOnly();
                break;
            case EnemyType.Hybrid:
                AttackHybrid();
                break;
            case EnemyType.CanShoot:
                AttackPlayerOnly();
                break;
        }
        // 移動速度に応じてアニメーション変更
        speed = agent.velocity.magnitude;
        _animator.SetFloat("Speed", speed);
    }

    // Wave開始時にステータスを設定
    public void Initialize(int waveNum, float waveMaxHP, float waveDamage)
    {
        waveNumber = waveNum;
        maxHP = waveMaxHP;
        currentHP = maxHP;
        damage = waveDamage;
        // ランダムで行動タイプを決定
        enemyType = (EnemyType)Random.Range(0, 4);
        canShoot = (enemyType == EnemyType.CanShoot);
    }

    // タワーだけを攻撃する敵
    private void AttackTowerOnly()
    {
        if (targetTower == null) { return; }
        agent.SetDestination(targetTower.position);
        Tower tower = targetTower.GetComponent<Tower>();
        if (!agent.pathPending && tower != null && agent.remainingDistance <= agent.stoppingDistance && attackTimer >= attackRate + tower.getSlowRate())
        {
            tower.TakeDamage(damage);
            attackTimer = 0f;
            _animator.SetTrigger("Attack");
        }
    }
    // プレイヤーだけを攻撃
    private void AttackPlayerOnly()
    {
        if (player == null) { return; }
        agent.SetDestination(player.position);
        Tower tower = targetTower.GetComponent<Tower>();
        // 近接攻撃
        if (!agent.pathPending && tower != null && agent.remainingDistance <= agent.stoppingDistance && attackTimer >= attackRate + tower.getSlowRate() && !canShoot)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) { ph.TakeDamage(damage); }
            attackTimer = 0f;
            _animator.SetTrigger("Attack");
        }
        // 射撃攻撃
        else if (!agent.pathPending && attackTimer >= attackRate + tower.getSlowRate() + 2f && canShoot)
        {
            FireBulletAtPlayer();
            attackTimer = 0f;
        }
    }
    // 射撃処理
    private void FireBulletAtPlayer()
    {
        if (player == null || bulletPrefab == null || shootPoint == null) { return; }
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        Vector3 target = player.position + new Vector3(0f, 2f, 0f);
        Vector3 direction = (target - shootPoint.position).normalized;
        rb.linearVelocity = direction * bulletSpeed;
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        _animator.SetTrigger("Attack");
        if (bulletComp != null)
        {
            bulletComp.setEnemyBullet(damage);
        }
        Destroy(bullet, bulletLifetime);
        AudioManager.instance.PlaySoundAtPosition(AudioManager.instance.audioClips.enemyFireSound, transform.position, 0.4f, 0.95f, 1.05f, 100f);
    }
    // プレイヤーとタワーを状況で切り替える敵
    private void AttackHybrid()
    {
        if (player == null || targetTower == null) { return; }
        Tower tower = targetTower.GetComponent<Tower>();
        float playerDist = Vector3.Distance(transform.position, player.position);
        float towerDist = Vector3.Distance(transform.position, targetTower.position);
        // プレイヤーが近い → プレイヤー攻撃
        if (playerDist < towerDist * 2)
        {
            agent.SetDestination(player.position);
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && attackTimer >= attackRate + tower.getSlowRate())
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) { ph.TakeDamage(damage); }
                attackTimer = 0f;
                _animator.SetTrigger("Attack");
            }
        }
        // タワーが近い → タワー攻撃
        else
        {
            agent.SetDestination(targetTower.position);
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && attackTimer >= attackRate + tower.getSlowRate())
            {
                tower.TakeDamage(damage);
                attackTimer = 0f;
                _animator.SetTrigger("Attack");
            }
        }
    }
    // 被ダメージ処理
    public void TakeDamage(float amount, bool isPlayer, bool isCritical)
    {
        if (isDead) { return; }
        currentHP -= amount;
        AudioManager.instance.PlaySoundAtPosition(AudioManager.instance.audioClips.enemyDamagedSound, transform.position, 0.7f, 0.95f, 1.05f, 100f);
        ShowDamageText(amount, isCritical);
        if (currentHP <= 0)
        {
            isDead = true;
            Die(isPlayer);
            AudioManager.instance.PlaySoundAtPosition(
                AudioManager.instance.audioClips.enemyDeathSound,
                transform.position,
                0.2f, 0.95f, 1.05f, 30f
            );
        }
    }

    // ダメージテキスト表示
    private void ShowDamageText(float amount, bool isCritical)
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(-0.5f, 0.5f),
            Random.Range(0f, 1f)
        );
        Vector3 spawnPosition = transform.position + Vector3.up * 4f + randomOffset;
        GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
        TextMeshProUGUI text = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"{Mathf.RoundToInt(amount)}";
        // プレイヤー方向に向ける
        if (player != null)
        {
            Vector3 lookDirection = damageTextObj.transform.position - player.position;
            lookDirection.y = 0;
            damageTextObj.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        // 距離に応じてスケール調整
        Camera cam = Camera.main;
        float distance = Vector3.Distance(cam.transform.position, spawnPosition);
        float baseScale = 1 + distance * 0.02f;
        float finalScale = isCritical ? baseScale * 1.5f : baseScale;
        damageTextObj.transform.localScale = Vector3.one * finalScale;
        Destroy(damageTextObj, 0.5f);
    }
    // 死亡処理
    private void Die(bool isPlayer)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDefeated();
        }
        // プレイヤーが倒した場合のみドロップ
        if (isPlayer)
        {
            ItemList.instance.dropItems(waveNumber);
        }
        Destroy(gameObject);
        _animator.SetTrigger("Die");
    }
}