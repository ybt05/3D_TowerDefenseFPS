//TitleScene AccessoryManagerObject 
//GameClearScene AccessoryManagerObject 
//GameOverScene AccessoryManagerObject 
using System.Collections.Generic;
using UnityEngine;

// ステータス1つ分のデータ
[System.Serializable]
public class StatEntry
{
    public string statName;
    public float value;
}
// アクセサリー1個分のデータ
[System.Serializable]
public class AccessoryData
{
    public string accessoryName; // アクセサリー名
    public StatEntry mainStat; // メインステータス
    public List<StatEntry> subStats = new List<StatEntry>(); // サブステータス
}
// プレイヤーのアクセサリー所持・装備データ
[System.Serializable]
public class AccessoryInventory
{
    public List<AccessoryData> ownedAccessories = new List<AccessoryData>();
    public List<AccessoryData> equippedAccessories = new List<AccessoryData>(new AccessoryData[5]);
}
// アクセサリー生成・保存・読み込みを管理するクラス
public class AccessoryManager : MonoBehaviour
{
    private const string PlayerPrefsKey = "accessoryInventory";
    private Dictionary<string, (string statName, Dictionary<string, float[]>)> mainStatMap;
    private Dictionary<string, (string statName, Dictionary<string, float[]>)> subStatMap;
    private Dictionary<string, string> accessoryToMainStat;

    private void Awake()
    {
        InitializeStatData();
    }
    // ランダムなアクセサリーを生成する
    public AccessoryData GenerateRandomAccessory()
    {
        // 必要な辞書が未初期化なら再初期化
        if (mainStatMap == null || accessoryToMainStat == null || subStatMap == null)
        {
            InitializeStatData();
        }
        // 難易度取得
        string difficulty = MainManager.instance != null ? MainManager.instance.getDifficulty() : "Easy";
        if (difficulty != "Easy" && difficulty != "Normal" && difficulty != "Hard")
        {
            difficulty = "Easy";
        }
        // ランダムにアクセサリー名を選択
        List<string> accessoryNames = new List<string>(accessoryToMainStat.Keys);
        string accessoryName = accessoryNames[Random.Range(0, accessoryNames.Count)];
        // メインステータス決定
        string mainStatKey = accessoryToMainStat[accessoryName];
        float[] mainOptions = mainStatMap[mainStatKey].Item2[difficulty];
        float mainValue = mainOptions[Random.Range(0, mainOptions.Length)];
        var mainStat = new StatEntry
        {
            statName = mainStatMap[mainStatKey].statName,
            value = mainValue
        };
        // サブステータスをランダムに5種類選ぶ（メインと同じものは除外）
        List<string> subStatKeys = new List<string>(subStatMap.Keys);
        subStatKeys.Remove(mainStatKey); // メインと同じステータスは除外
        List<string> selectedKeys = new List<string>();
        while (selectedKeys.Count < 5)
        {
            string candidate = subStatKeys[Random.Range(0, subStatKeys.Count)];
            if (!selectedKeys.Contains(candidate))
            {
                selectedKeys.Add(candidate);
            }
        }
        List<StatEntry> subStats = new List<StatEntry>();
        foreach (var key in selectedKeys)
        {
            float[] options = subStatMap[key].Item2[difficulty];
            float value = options[Random.Range(0, options.Length)];
            subStats.Add(new StatEntry { statName = subStatMap[key].statName, value = value });
        }
        // 完成したアクセサリーデータを返す
        return new AccessoryData
        {
            accessoryName = accessoryName,
            mainStat = mainStat,
            subStats = subStats
        };
    }
    // 所持アクセサリー一覧をPlayerPrefsから読み込む
    public List<AccessoryData> LoadOwnedAccessories()
    {
        if (!PlayerPrefs.HasKey("accessoryInventory"))
        {
            Debug.Log("AccessoryManager.cs PlayerPrefsにアクセサリーデータが存在しません");
            return new List<AccessoryData>();
        }
        try
        {
            string encrypted = PlayerPrefs.GetString("accessoryInventory");
            if (string.IsNullOrWhiteSpace(encrypted))
            {
                Debug.LogWarning("AccessoryManager.cs PlayerPrefsから読み込んだデータが空です");
                return new List<AccessoryData>();
            }
            string json = CryptoUtility.Decrypt(encrypted);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("AccessoryManager.cs 復号後のJSONが空です");
                return new List<AccessoryData>();
            }
            var inv = JsonUtility.FromJson<AccessoryInventory>(json);
            return inv?.ownedAccessories ?? new List<AccessoryData>();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("AccessoryManager.cs PlayerPrefs読み込みエラー: " + ex.Message);
            return new List<AccessoryData>();
        }
    }
    // アクセサリー所持・装備データをPlayerPrefsに保存
    public void SaveAccessories(AccessoryInventory inventory)
    {
        string json = JsonUtility.ToJson(inventory);
        string encrypted = CryptoUtility.Encrypt(json);
        PlayerPrefs.SetString(PlayerPrefsKey, encrypted);
        PlayerPrefs.Save();
        Debug.Log("AccessoryManager.cs PlayerPrefsに保存完了");
    }
    // 所持+装備データをまとめて読み込む
    public AccessoryInventory LoadAccessoriesEncrypted()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            Debug.Log("AccessoryManager.cs PlayerPrefsにデータがありません");
            return new AccessoryInventory();
        }
        string encrypted = PlayerPrefs.GetString(PlayerPrefsKey);
        string json = CryptoUtility.Decrypt(encrypted);
        var inv = JsonUtility.FromJson<AccessoryInventory>(json);
        while (inv.equippedAccessories.Count < 5)
        {
            inv.equippedAccessories.Add(null);
        }
        if (inv.equippedAccessories.Count > 5)
        {
            inv.equippedAccessories = inv.equippedAccessories.GetRange(0, 5);
        }
        return inv;
    }

    private void InitializeStatData()
    {
        // アクセサリー名→メインステータス種別の対応表
        accessoryToMainStat = new Dictionary<string, string>
        {
            { "十字架のネックレス", "無敵時間アップ" },
            { "ハートの指輪", "自然回復" },
            { "神話のブーツ", "移動速度アップ" },
            { "貴族の時計", "復活時間短縮" },
            { "ローズの花飾り", "HPアップ" },
            { "不死鳥の羽", "攻撃力アップ" },
            { "聖騎士の盾", "防御力アップ" },
            { "幸運のピアス", "会心率アップ" },
            { "古代の冠", "会心ダメージアップ" },
        };
        // メインステータスの定義
        mainStatMap = new Dictionary<string, (string, Dictionary<string, float[]>)>
        {
            { "HPアップ", ("HP", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 3f, 5f} },
                    {"Normal",new float[]{ 8f, 10f}},
                    {"Hard",new float[]{ 12f, 15f} }
                })
            },
            { "攻撃力アップ", ("攻撃力", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 3f, 5f} },
                    {"Normal",new float[]{ 8f, 10f}},
                    {"Hard",new float[]{ 12f, 15f} }
                })
            },
            { "防御力アップ", ("防御力", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 1f, 2f} },
                    {"Normal",new float[]{ 3f, 4f}},
                    {"Hard",new float[]{ 5f, 6f} }
                })
            },
            { "会心率アップ", ("会心率", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 5f, 10f} },
                    {"Normal",new float[]{ 15f, 20f}},
                    {"Hard",new float[]{ 25f, 30f} }
                })
            },
            { "会心ダメージアップ", ("会心ダメージ", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 10f, 20f} },
                    {"Normal",new float[]{ 30f, 40f}},
                    {"Hard",new float[]{ 50f, 60f} }
                })
            },
            { "自然回復", ("自然回復", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.5f, 1f} },
                    {"Normal",new float[]{ 1.5f, 2f}},
                    {"Hard",new float[]{ 2.5f, 3f} }
                })
            },
            { "移動速度アップ", ("移動速度", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 3f, 5f} },
                    {"Normal",new float[]{ 8f, 10f}},
                    {"Hard",new float[]{ 12f, 15f} }
                })
            },
            { "無敵時間アップ", ("無敵時間", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.1f, 0.2f} },
                    {"Normal",new float[]{ 0.3f, 0.5f}},
                    {"Hard",new float[]{ 0.7f, 1f} }
                })
            },
            { "復活時間短縮", ("復活時間短縮", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.1f, 0.2f} },
                    {"Normal",new float[]{ 0.3f, 0.4f}},
                    {"Hard",new float[]{ 0.5f, 0.6f} }
                })
            },
        };
        // サブステータスの定義
        subStatMap = new Dictionary<string, (string, Dictionary<string, float[]>)>
        {
            { "HPアップ", ("HP", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 1f, 2f} },
                    {"Normal",new float[]{ 3f, 4f}},
                    {"Hard",new float[]{ 5f, 6f} }
                })
            },
            { "攻撃力アップ", ("攻撃力", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 1f, 2f} },
                    {"Normal",new float[]{ 3f, 4f}},
                    {"Hard",new float[]{ 5f, 6f} }
                })
            },
            { "防御力アップ", ("防御力", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.25f, 0.5f} },
                    {"Normal",new float[]{ 1f, 1.5f}},
                    {"Hard",new float[]{ 2f, 3f} }
                })
            },
            { "会心率アップ", ("会心率", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 2f, 3f} },
                    {"Normal",new float[]{ 5f, 8f}},
                    {"Hard",new float[]{ 10f, 15f} }
                })
            },
            { "会心ダメージアップ", ("会心ダメージ", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 4f, 6f} },
                    {"Normal",new float[]{ 10f, 14f}},
                    {"Hard",new float[]{ 20f, 30f} }
                })
            },
            { "自然回復", ("自然回復", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.25f, 0.5f} },
                    {"Normal",new float[]{ 1f, 1.5f}},
                    {"Hard",new float[]{ 2f, 3f} }
                })
            },
            { "移動速度アップ", ("移動速度", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 1f, 2f} },
                    {"Normal",new float[]{ 3f, 4f}},
                    {"Hard",new float[]{ 6f, 8f} }
                })
            },
            { "無敵時間アップ", ("無敵時間", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.01f, 0.02f} },
                    {"Normal",new float[]{ 0.03f, 0.05f}},
                    {"Hard",new float[]{ 0.07f, 0.1f} }
                })
            },
            { "復活時間短縮", ("復活時間短縮", new Dictionary<string, float[]>
                {
                    {"Easy",new float[]{ 0.01f, 0.02f} },
                    {"Normal",new float[]{ 0.03f, 0.04f}},
                    {"Hard",new float[]{ 0.05f, 0.06f} }
                })
            },
        };
    }
    // 初期アクセサリーをPlayerPrefsに保存（デバッグ用）
    [ContextMenu("初期アクセサリー保存")]
    public void SaveInitialAccessories()
    {
        InitializeStatData();
        AccessoryInventory inv = new AccessoryInventory();
        // 所持アクセサリーに10個追加
        for (int i = 0; i < 10; i++)
        {
            inv.ownedAccessories.Add(GenerateRandomAccessory());
        }
        inv.equippedAccessories = new List<AccessoryData>(new AccessoryData[5]);
        SaveAccessories(inv);
    }
}