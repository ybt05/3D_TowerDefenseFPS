// TitleScene MainManagerObject
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager instance;

    // 選択された難易度
    private string selectedDifficulty;

    // 選択されたジャンル
    private string selectedGenre = "literacy";

    // 装備中アクセサリー（最大5個）
    public List<AccessoryData> equippedAccessories = new List<AccessoryData>();

    // 所持アクセサリー一覧
    public List<AccessoryData> ownedAccessories = new List<AccessoryData>();

    // 最終スコア
    private float finalScore = 0;

    // 到達Wave
    private int wave = 0;

    [SerializeField] private GameObject mainPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも保持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ゲーム開始時に状態をリセット
    public void ResetGameState()
    {
        finalScore = 0f;
        wave = 0;
        selectedDifficulty = "";
        selectedGenre = "literacy";
        // アクセサリー情報も初期化
        equippedAccessories = new List<AccessoryData>();
        ownedAccessories = new List<AccessoryData>();
    }

    // スコア更新
    public void changeScore(float score)
    {
        finalScore = score;
    }

    // スコア取得
    public float getScore()
    {
        return finalScore;
    }

    // スコアを直接セット
    public void setScore(float score)
    {
        finalScore = score;
    }

    // ジャンル取得
    public string getGenre()
    {
        return selectedGenre;
    }

    // ジャンル設定
    public void setGenre(string genre)
    {
        selectedGenre = genre;
    }

    // 難易度取得
    public string getDifficulty()
    {
        return selectedDifficulty;
    }

    // 難易度設定
    public void setDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
    }

    // 現在のWave取得
    public int getWave()
    {
        return wave;
    }

    // Wave更新
    public void changeWave(int wave)
    {
        this.wave = wave;
    }
}