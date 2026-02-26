//MainScene AudioManagerObject
//TitleScene AudioManagerObject
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioClips audioClips;
    [Header("Pool settings")]
    [SerializeField] private int audioSourcePoolSize = 12; // 同時再生できる効果音数
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f; // 効果音の基本音量
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.1f; // BGMの基本音量

    private AudioSource[] pool;     // 効果音用AudioSourceプール(オブジェクトを事前に作り置きして使い回す)
    private int poolIndex = 0;      // 次に使うAudioSourceのインデックス
    private AudioSource bgmSource;  // BGM再生専用AudioSource
    private Coroutine fadeCoroutine; // フェード処理の同時実行防止用
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄しない
            InitializePool();              // 効果音プール初期化
            InitializeBGMSource();         // BGM用AudioSource初期化
        }
        else
        {
            Destroy(gameObject); // 2つ目以降は破棄
        }
    }
    // 効果音再生用のAudioSourceを複数まとめて保持し、順番に使う
    private void InitializePool()
    {
        pool = new AudioSource[Mathf.Max(1, audioSourcePoolSize)];
        for (int i = 0; i < pool.Length; i++)
        {
            GameObject go = new GameObject($"SFX_Source_{i}");
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            pool[i] = src;
        }
        poolIndex = 0;
    }
    // BGM再生専用AudioSourceを作成
    private void InitializeBGMSource()
    {
        GameObject go = new GameObject("BGM_Source");
        go.transform.SetParent(transform);
        bgmSource = go.AddComponent<AudioSource>();
        bgmSource.loop = true;       // BGMはループ
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // 2D
    }

    // 2D効果音を再生
    public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || pool == null || pool.Length == 0) { return; }
        AudioSource src = pool[poolIndex];
        src.volume = Mathf.Clamp01(sfxVolume) * Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.PlayOneShot(clip);
        poolIndex = (poolIndex + 1) % pool.Length;
    }
    // ランダムピッチ付きで再生
    public void PlaySoundRandomPitch(AudioClip clip, float volume = 1f, float minPitch = 0.98f, float maxPitch = 1.02f)
    {
        float p = Random.Range(minPitch, maxPitch);
        PlaySound(clip, volume, p);
    }
    // 3Dで再生
    public void PlaySoundAtPosition(AudioClip clip, Vector3 pos, float volume = 1f, float minPitch = 1f, float maxPitch = 1f, float maxDistance = 20f)
    {
        if (clip == null) { return; }
        GameObject go = new GameObject("Temp3DSFX");
        go.transform.position = pos;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f; // 3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.maxDistance = maxDistance;
        src.volume = Mathf.Clamp01(sfxVolume) * Mathf.Clamp01(volume);
        src.pitch = Random.Range(minPitch, maxPitch);
        src.Play();
        Destroy(go, clip.length / src.pitch + 0.1f);
    }
    // BGMフェードイン
    public void PlayBGMWithFade(AudioClip clip, float fadeTime = 1f)
    {
        if (clip == null) { return; }
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInCoroutine(clip, fadeTime));
    }
    private IEnumerator FadeInCoroutine(AudioClip clip, float fadeTime)
    {
        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / fadeTime);
            yield return null;
        }
        bgmSource.volume = bgmVolume;
    }

    // BGMフェードアウト
    public void StopBGMWithFade(float fadeTime = 1f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutCoroutine(fadeTime));
    }
    private IEnumerator FadeOutCoroutine(float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
    }
}
