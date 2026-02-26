// Prefabs/Turret
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private Transform firePoint;   // 弾の発射位置
    [SerializeField] private GameObject bulletPrefab; // 発射する弾のプレハブ
    [SerializeField] private float fireRate = 1f;   // 発射間隔（秒）
    [SerializeField] private float range = 10f;     // 敵を探す範囲
    [SerializeField] private float bulletSpeed = 100f; // 弾速
    [SerializeField] private LayerMask enemyLayer;  // 敵レイヤー
    [SerializeField] private float damage = 80f;    // タレットの与ダメージ
    private Transform target;        // 現在狙っている敵
    private float fireCountdown = 0f; // 発射クールダウン
    void Update()
    {
        FindTarget(); // 最も近い敵を探す

        if (target != null)
        {
            // 敵の方向を向く
            transform.LookAt(target);
            // 発射
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = fireRate;
            }
            fireCountdown -= Time.deltaTime;
        }
    }
    // 範囲内の最も近い敵を探索
    void FindTarget()
    {
        // 範囲内の敵を取得
        Collider[] enemies = Physics.OverlapSphere(transform.position, range, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        // 最も近い敵を探す
        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
        target = nearestEnemy;
    }
    // 弾を発射する
    void Shoot()
    {
        // 弾を生成
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // 弾に速度を与える
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * bulletSpeed;
        // 弾にタレット弾であることを伝える
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.setTurretBullet(damage);
        }
    }
}