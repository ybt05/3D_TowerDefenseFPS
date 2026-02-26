//MainScene Player
using UnityEngine;
public class FootstepSound : MonoBehaviour
{
    public AudioClip[] footstepClips;   // 足音として再生するクリップ一覧
    public float stepInterval = 0.5f;   // 足音を鳴らす間隔（秒）
    public float stepVolume = 0.7f;     // 足音の音量
    private AudioSource audioSource;    // 足音再生用AudioSource
    private CharacterController controller; // プレイヤーの移動判定に使用
    private float stepTimer;            // 足音間隔を測るタイマー
    private bool isGrounded;            // 接地状態

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        // CharacterControllerを取得（移動速度や接地判定に使用）
        controller = GetComponent<CharacterController>();
        audioSource.spatialBlend = 1f; // 3D
    }
    void Update()
    {
        // 接地判定
        isGrounded = controller.isGrounded;
        // ジャンプ中は鳴らさない
        if (!isGrounded) { return; }
        // UIを開いている時は足音を鳴らさない
        if (PlayerController.instance.getIsTowerOpen()) { return; }
        // 動いている場合に足音タイマー進行
        if (controller.velocity.magnitude > 0.2f)
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
        // ピッチを揺らして自然さを出す
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        // 音量設定
        audioSource.volume = stepVolume;
        // 再生
        audioSource.PlayOneShot(footstepClips[index]);
    }

}
