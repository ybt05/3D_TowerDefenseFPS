// Prefabs/Bullet
// Prefabs/Magic
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage = 0;          // この弾が与えるダメージ
    private bool isCritical = false;   // クリティカルヒットかどうか
    private bool isPlayerBullet = false; // プレイヤーが撃った弾
    private bool isEnemyBullet = false;  // 敵が撃った弾
    private bool isTurretBullet = false; // タレットが撃った弾

    [SerializeField] private GameObject impactEffectPrefab; // 着弾エフェクト
    [SerializeField] private float effectTime = 2f;         // エフェクトの寿命

    [Header("爆発設定")]
    [SerializeField] private float explosionRadius = 5f;    // 爆発範囲
    [SerializeField] private LayerMask enemyLayer;          // 敵レイヤー

    // プレイヤーの弾として設定（PlayerController から呼ばれる）
    public void setPlayerBullet()
    {
        isPlayerBullet = true;
    }

    // 敵の弾として設定（Enemy.cs から呼ばれる）
    public void setEnemyBullet(float dmg)
    {
        damage = dmg;
        isEnemyBullet = true;
    }

    // タレット弾として設定（Turret.cs から呼ばれる）
    public void setTurretBullet(float dmg)
    {
        damage = dmg;
        isTurretBullet = true;
    }

    // 弾と他のCollider付きオブジェクトが衝突したら呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = transform.position; // 衝突地点
        Quaternion hitRotation = Quaternion.identity; // 衝突時の回転
        // エフェクトを生成
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, hitPoint, hitRotation);
            Destroy(effect, effectTime);
        }
        // 爆発ダメージ処理
        if (ItemList.instance.isExplosive() && isPlayerBullet)
        {
            Explode(hitPoint);
        }
        // タワーと接触したら何もせず削除
        if (other.CompareTag("Tower"))
        {
            Destroy(gameObject);
        }
        // 敵の弾の処理
        if (isEnemyBullet)
        {
            // プレイヤーと接触したらダメージを与える
            if (other.CompareTag("Player"))
            {
                PlayerHealth ph = other.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            // プレイヤーじゃなかったら何もせず削除
            else
            {
                Destroy(gameObject);
            }
        }
        // タレットの弾の処理
        else if (isTurretBullet)
        {
            // 敵と接触したらダメージを与える
            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, true, false);
                }
                Destroy(gameObject);
            }
            // 敵じゃなかったら何もせず削除
            else
            {
                Destroy(gameObject);
            }
        }
        // プレイヤーの弾の処理
        else if (isPlayerBullet)
        {
            // ダメージ取得
            var result = PlayerHealth.instance.GetDamage();
            damage = result.damage;
            isCritical = result.isCritical;
            // 敵と接触したらダメージを与える
            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, true, isCritical);
                }
                Destroy(gameObject);
            }
            // 敵じゃなかったら何もせず削除
            else
            {
                Destroy(gameObject);
            }
        }
    }
    // 爆発ダメージ処理
    private void Explode(Vector3 center)
    {
        // 爆発範囲にあるColliderをすべて取得する
        Collider[] hitColliders = Physics.OverlapSphere(center, explosionRadius, enemyLayer);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            // 敵に爆発ダメージを与える
            if (enemy != null)
            {
                var result = PlayerHealth.instance.GetDamage();
                damage = result.damage;
                isCritical = result.isCritical;
                enemy.TakeDamage(damage, true, isCritical);
            }
        }
    }
}