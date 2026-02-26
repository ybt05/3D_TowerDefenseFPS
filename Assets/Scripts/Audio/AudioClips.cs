// Assets
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClips", menuName = "Audio/AudioClips")]
public class AudioClips : ScriptableObject
{
    [Header("プレイヤー関連")]
    public AudioClip shootSound;
    public AudioClip eatSound;
    public AudioClip playerDamagedSound;
    public AudioClip playerDeathSound;

    [Header("タワー関連")]
    public AudioClip towerDamagedSound;
    public AudioClip towerDestroySound;
    public AudioClip towerAttackSound;

    [Header("UI関連")]
    public AudioClip decisionSound;
    public AudioClip cancelSound;
    public AudioClip changeWeaponSound;
    public AudioClip levelUpSound;
    [Header("敵関連")]
    public AudioClip enemyDamagedSound;
    public AudioClip enemyDeathSound;
    public AudioClip enemyFireSound;
    [Header("ゲーム関連")]
    public AudioClip startSound;
    public AudioClip endSound;
    [Header("BGM")]
    public AudioClip bgmSound;
}
