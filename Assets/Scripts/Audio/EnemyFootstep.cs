// Prefabs/MonsterPrefab
using UnityEngine;
using UnityEngine.AI;

public class EnemyFootstep : MonoBehaviour
{
public AudioClip[] footstepClips;   // 足音として再生するクリップ一覧
    public float stepInterval = 0.6f;   // 足音を鳴らす間隔（秒）
    public float stepVolume = 2.0f;     // 足音の音量
    private NavMeshAgent agent;         // 敵の移動を制御するNavMeshAgent
    private AudioSource audioSource;    // 足音再生用AudioSource
    private float stepTimer;            // 足音間隔を測るタイマー
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D
    }
    void Update()
    {
        // 移動中のみ足音再生
        // 速度が一定以上で目的地に向かって移動中
        if (agent.velocity.magnitude > 0.2f && agent.remainingDistance > agent.stoppingDistance)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }
    // 足音を1回再生
    void PlayFootstep()
    {
        if (footstepClips.Length == 0) { return; }
        // ランダムな足音を選択
        int index = Random.Range(0, footstepClips.Length);
        // ピッチを少し揺らして自然さを出す
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        // 音量設定
        audioSource.volume = stepVolume;
        // 再生
        audioSource.PlayOneShot(footstepClips[index]);
    }
}
