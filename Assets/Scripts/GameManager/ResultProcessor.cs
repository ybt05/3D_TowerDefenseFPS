// GameClearScene GameClearManagerObject
// GameOverScene GameOverManagerObject
using UnityEngine;
using System.Collections.Generic;

public class ResultProcessor : MonoBehaviour
{
    // スコアに応じてコインを付与
    public static void GrantCoins()
    {
        int score = (int)MainManager.instance.getScore();
        int current = PlayerPrefs.GetInt("coins", 0);
        PlayerPrefs.SetInt("coins", current + score);
        PlayerPrefs.Save();
    }
    // Wave数に応じてランダムアクセサリーを付与
    public static void GrantRewardAccessories()
    {
        int count = GetRewardCount();
        var manager = FindFirstObjectByType<AccessoryManager>();
        if (manager == null) { return; }
        for (int i = 0; i < count; i++)
        {
            var acc = manager.GenerateRandomAccessory();
            MainManager.instance.ownedAccessories.Add(acc);
        }
    }
    // アクセサリー所持データを保存
    public static void SaveAccessories()
    {
        var manager = FindFirstObjectByType<AccessoryManager>();
        if (manager == null) { return; }
        // 既存データを読み込み
        AccessoryInventory loaded = manager.LoadAccessoriesEncrypted();
        if (loaded == null)
        {
            loaded = new AccessoryInventory
            {
                ownedAccessories = new List<AccessoryData>(),
                equippedAccessories = new List<AccessoryData>(new AccessoryData[5])
            };
        }
        // 新しく獲得したアクセサリーを追加
        foreach (var acc in MainManager.instance.ownedAccessories)
        {
            loaded.ownedAccessories.Add(acc);
        }
        // 装備情報も保存
        loaded.equippedAccessories = new List<AccessoryData>(MainManager.instance.equippedAccessories);
        // 保存
        string json = JsonUtility.ToJson(loaded);
        string encrypted = CryptoUtility.Encrypt(json);
        PlayerPrefs.SetString("accessoryInventory", encrypted);
        PlayerPrefs.Save();
    }
    // スコアお守り補正を適用した最終スコアを計算
    public static float CalculateFinalScore()
    {
        float baseScore = MainManager.instance.getScore();
        int charm = PlayerPrefs.GetInt("Charm_Score", 0);
        float bonus = 1f + charm * 0.001f;
        float finalScore = baseScore * bonus;
        MainManager.instance.setScore(finalScore);
        return finalScore;
    }

    // スコア履歴を保存
    public static void SaveScoreHistory()
    {
        GameClearManager.ScoreData data;
        if (PlayerPrefs.HasKey("scoreData"))
        {
            string encrypted = PlayerPrefs.GetString("scoreData");
            string json = CryptoUtility.Decrypt(encrypted);
            data = JsonUtility.FromJson<GameClearManager.ScoreData>(json);
        }
        else
        {
            data = new GameClearManager.ScoreData();
        }
        // 新しいスコアを追加
        data.entries.Add(new ScoreEntry(
            (int)MainManager.instance.getScore(),
            MainManager.instance.getGenre()
        ));
        // 再保存
        string newJson = JsonUtility.ToJson(data);
        string newEncrypted = CryptoUtility.Encrypt(newJson);
        PlayerPrefs.SetString("scoreData", newEncrypted);
        PlayerPrefs.Save();
    }
    // Wave数に応じた報酬アクセサリー数
    public static int GetRewardCount()
    {
        int wave = MainManager.instance.getWave();
        if (wave >= 9) { return 3; }
        if (wave >= 6) { return 2; }
        if (wave >= 4) { return 1; }
        return 0;
    }
    // アクセサリー情報をUI用の文字列に整形
    public static string FormatAccessoryText(AccessoryData acc)
    {
        string main =
            $"<color=#0011FF>{acc.accessoryName}</color> " +
            $"<color=#FF4444>{acc.mainStat.statName}: {acc.mainStat.value}</color>\n";
        string subs = "";
        foreach (var sub in acc.subStats)
        {
            subs += $"<color=#228B22>{sub.statName}:{sub.value}</color> ";
        }
        return main + subs;
    }
}