// Prefabs/DamageText/Text(TMP)
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;   // テキストが上昇する速度
    [SerializeField] private float lifetime = 0.5f;   // 何秒後に消えるか
    private Vector3 moveDirection = Vector3.up;       // 上方向へ移動

    void Update()
    {
        // 毎フレーム、上方向へ移動させる
        transform.position += moveDirection * floatSpeed * Time.deltaTime;
    }

    void Start()
    {
        // 一定時間後に自動で削除
        Destroy(gameObject, lifetime);
    }
}