//MainScene Manager/ItemListManagerObject
using UnityEngine;
using System.Collections.Generic;
// アイテムの種類分類
public enum ItemType { Money, Material, Gun, Bullet, Food, Turret }
public class ItemList : MonoBehaviour
{
    public static ItemList instance;
    // クラフト可能なアイテム一覧
    public string[] CraftableItemName = { "ハンドガン", "ショットガン", "サブマシンガン", "アサルトライフル", "スナイパー", "ライトマシンガン", "鉄の弾", "炸裂弾", "ベイクドポテト", "シチュー", "タレット" };
    // 武器一覧
    public string[] GunName = { "ピストル", "ハンドガン", "ショットガン", "サブマシンガン", "アサルトライフル", "スナイパー", "ライトマシンガン" };
    // 敵がドロップする素材一覧
    [SerializeField] private string[] DropItemName = { "コイン", "木材", "小石", "鉄", "金", "火薬", "ポテト", "ニンジン" };
    // 各アイテムの所持数・ドロップ率・クラフト素材などのデータ
    [Header("コイン")]
    [SerializeField] private int Coin = 0;
    [SerializeField] private float CoinDrop = 1;
    [SerializeField] private int CoinMin = 100;
    [SerializeField] private int CoinMax = 200;
    [Header("木材")]
    [SerializeField] private int Wood = 0;
    [SerializeField] private float WoodDrop = 0.5f;
    [SerializeField] private int WoodMin = 1;
    [SerializeField] private int WoodMax = 2;
    [Header("小石")]
    [SerializeField] private int Stone = 100;
    [SerializeField] private float StoneDrop = 0.6f;
    [SerializeField] private int StoneMin = 10;
    [SerializeField] private int StoneMax = 20;
    [SerializeField] private int StoneBulletDamage = 5;
    [SerializeField] private bool StoneBulletIsExplosive = false;
    [Header("鉄")]
    [SerializeField] private int Iron = 0;
    [SerializeField] private float IronDrop = 0.8f;
    [SerializeField] private int IronMin = 1;
    [SerializeField] private int IronMax = 2;
    [Header("金")]
    [SerializeField] private int Gold = 0;
    [SerializeField] private float GoldDrop = 0.1f;
    [SerializeField] private int GoldMin = 1;
    [SerializeField] private int GoldMax = 1;
    [Header("火薬")]
    [SerializeField] private int Gunpowder = 0;
    [SerializeField] private float GunpowderDrop = 0.2f;
    [SerializeField] private int GunpowderMin = 1;
    [SerializeField] private int GunpowderMax = 2;
    [Header("ポテト")]
    [SerializeField] private int Potato = 0;
    [SerializeField] private float PotatoDrop = 0.2f;
    [SerializeField] private int PotatoMin = 1;
    [SerializeField] private int PotatoMax = 2;
    [Header("ニンジン")]
    [SerializeField] private int Carrot = 0;
    [SerializeField] private float CarrotDrop = 0.1f;
    [SerializeField] private int CarrotMin = 1;
    [SerializeField] private int CarrotMax = 2;
    [Header("ピストル")]
    [SerializeField] private int Pistol = 1;
    [SerializeField] private int PistolDamage = 10;
    [SerializeField] private float PistolFireRate = 0.5f;
    [SerializeField] private bool PistolAutoFire = false;
    [Header("ハンドガン")]
    [SerializeField] private int Handgun = 0;
    [SerializeField] private int HandgunDamage = 30;
    [SerializeField] private float HandgunFireRate = 0.8f;
    [SerializeField] private bool HandgunAutoFire = false;
    private Dictionary<string, int> HandgunMaterial = new Dictionary<string, int>() { { "鉄", 5 } };
    [Header("ショットガン")]
    [SerializeField] private int Shotgun = 0;
    [SerializeField] private int ShotgunDamage = 5;
    [SerializeField] private float ShotgunFireRate = 1f;
    [SerializeField] private bool ShotgunAutoFire = false;
    private Dictionary<string, int> ShotgunMaterial = new Dictionary<string, int>() { { "鉄", 5 } };
    [Header("サブマシンガン")]
    [SerializeField] private int SubMachineGun = 0;
    [SerializeField] private int SubMachineGunDamage = 10;
    [SerializeField] private float SubMachineGunFireRate = 0.2f;
    [SerializeField] private bool SubMachineGunAutoFire = true;
    private Dictionary<string, int> SubMachineGunMaterial = new Dictionary<string, int>() { { "鉄", 15 } };
    [Header("アサルトライフル")]
    [SerializeField] private int AssaultRifle = 0;
    [SerializeField] private int AssaultRifleDamage = 50;
    [SerializeField] private float AssaultRifleFireRate = 0.3f;
    [SerializeField] private bool AssaultRifleAutoFire = true;
    private Dictionary<string, int> AssaultRifleMaterial = new Dictionary<string, int>() { { "鉄", 25 } };
    [Header("スナイパー")]
    [SerializeField] private int SniperRifle = 0;
    [SerializeField] private int SniperRifleDamage = 500;
    [SerializeField] private float SniperRifleFireRate = 2.0f;
    [SerializeField] private bool SniperRifleAutoFire = false;
    private Dictionary<string, int> SniperRifleMaterial = new Dictionary<string, int>() { { "鉄", 30 }, { "金", 5 } };
    [Header("ライトマシンガン")]
    [SerializeField] private int LightMachineGun = 0;
    [SerializeField] private int LightMachineGunDamage = 50;
    [SerializeField] private float LightMachineGunFireRate = 0.2f;
    [SerializeField] private bool LightMachineGunAutoFire = true;
    private Dictionary<string, int> LightMachineGunMaterial = new Dictionary<string, int>() { { "鉄", 50 }, { "金", 5 } };
    [Header("鉄の弾")]
    [SerializeField] private int IronBullet = 0;
    [SerializeField] private int IronBulletDamage = 10;
    [SerializeField] private int IronBulletsPerCraft = 20;
    [SerializeField] private bool IronBulletIsExplosive = false;
    private Dictionary<string, int> IronBulletMaterial = new Dictionary<string, int>() { { "鉄", 1 } };
    [Header("炸裂弾")]
    [SerializeField] private int ExplosiveBullet = 0;
    [SerializeField] private int ExplosiveBulletDamage = 15;
    [SerializeField] private int ExplosiveBulletsPerCraft = 20;
    [SerializeField] private bool ExplosiveBulletIsExplosive = true;
    private Dictionary<string, int> ExplosiveBulletMaterial = new Dictionary<string, int>() { { "鉄", 1 }, { "火薬", 1 } };
    [Header("ベイクドポテト")]
    [SerializeField] private int BakedPotato = 0;
    [SerializeField] private int BakedPotatoHeal = 10;
    private Dictionary<string, int> BakedPotatoMaterial = new Dictionary<string, int>() { { "ポテト", 1 }, { "木材", 2 } };
    [Header("シチュー")]
    [SerializeField] private int Stew = 0;
    [SerializeField] private int StewHeal = 30;
    private Dictionary<string, int> StewMaterial = new Dictionary<string, int>() { { "ポテト", 2 }, { "ニンジン", 2 }, { "木材", 3 } };
    [Header("タレット")]
    [SerializeField] private int Turret = 0;
    private Dictionary<string, int> TurretMaterial = new Dictionary<string, int>() { { "鉄", 50 }, { "金", 5 } };
    // 現在装備中の武器・弾薬の性能
    private string currentGun;
    private int currentGunDamage;
    private float currentGunFireRate;
    private bool currentGunAutoFire;
    private int currentBulletDamage;
    private bool isBulletExplosive;

    // 素材、武器、弾薬、料理、タレットを一括管理する
    // 辞書に入れて名前でアクセスできるようにする
    private struct Items
    {
        public string name;                     // アイテム名
        public int num;                         // 所持数
        public float dropNum;                   // ドロップ率
        public int dropMin;                     // ドロップ最小量
        public int dropMax;                     // ドロップ最大量
        public int perCraft;                    // クラフト時に生成される個数
        public int damage;                      // 武器・弾薬のダメージ
        public bool autoFire;                   // 武器のオート射撃可否
        public bool explosive;                  // 弾薬が爆発属性か
        public float fireRate;                  // 武器の連射速度
        public int heal;                        // 食べ物の回復量
        public Dictionary<string, int> material; // クラフト素材
        public ItemType type;                   // アイテムの種類
    }
    // 全アイテムを名前で管理する辞書
    private Dictionary<string, Items> items = new Dictionary<string, Items>();

    // 現在装備中の武器・弾薬の性能を返す
    public int getGunDamage() => currentGunDamage + currentBulletDamage;
    public bool isExplosive() => isBulletExplosive;
    public bool isAutoFire() => currentGunAutoFire;
    public float getFireRate() => currentGunFireRate;
    public string getGunName() => currentGun;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        InitializeItems();
        currentGun = "ピストル";
        currentGunDamage = PistolDamage;
        currentGunFireRate = PistolFireRate;
        currentGunAutoFire = PistolAutoFire;
    }
    // 全アイテムをItems構造体にまとめて辞書へ登録する
    private void InitializeItems()
    {
        items["コイン"] = new Items
        {
            name = "コイン",
            num = Coin,
            dropNum = CoinDrop,
            dropMin = CoinMin,
            dropMax = CoinMax,
            type = ItemType.Money
        };
        items["木材"] = new Items
        {
            name = "木材",
            num = Wood,
            dropNum = WoodDrop,
            dropMin = WoodMin,
            dropMax = WoodMax,
            type = ItemType.Material
        };
        items["小石"] = new Items
        {
            name = "小石",
            num = Stone,
            dropNum = StoneDrop,
            dropMin = StoneMin,
            dropMax = StoneMax,
            damage = StoneBulletDamage,
            explosive = StoneBulletIsExplosive,
            type = ItemType.Bullet
        };
        items["鉄"] = new Items
        {
            name = "鉄",
            num = Iron,
            dropNum = IronDrop,
            dropMin = IronMin,
            dropMax = IronMax,
            type = ItemType.Material
        };
        items["金"] = new Items
        {
            name = "金",
            num = Gold,
            dropNum = GoldDrop,
            dropMin = GoldMin,
            dropMax = GoldMax,
            type = ItemType.Material
        };
        items["火薬"] = new Items
        {
            name = "火薬",
            num = Gunpowder,
            dropNum = GunpowderDrop,
            dropMin = GunpowderMin,
            dropMax = GunpowderMax,
            type = ItemType.Material
        };
        items["ポテト"] = new Items
        {
            name = "ポテト",
            num = Potato,
            dropNum = PotatoDrop,
            dropMin = PotatoMin,
            dropMax = PotatoMax,
            type = ItemType.Material
        };
        items["ニンジン"] = new Items
        {
            name = "ニンジン",
            num = Carrot,
            dropNum = CarrotDrop,
            dropMin = CarrotMin,
            dropMax = CarrotMax,
            type = ItemType.Material
        };
        items["ピストル"] = new Items
        {
            name = "ピストル",
            num = Pistol,
            damage = PistolDamage,
            fireRate = PistolFireRate,
            autoFire = PistolAutoFire,
            type = ItemType.Gun,
        };
        items["ハンドガン"] = new Items
        {
            name = "ハンドガン",
            num = Handgun,
            damage = HandgunDamage,
            fireRate = HandgunFireRate,
            autoFire = HandgunAutoFire,
            type = ItemType.Gun,
            material = HandgunMaterial
        };
        items["ショットガン"] = new Items
        {
            name = "ショットガン",
            num = Shotgun,
            damage = ShotgunDamage,
            fireRate = ShotgunFireRate,
            autoFire = ShotgunAutoFire,
            type = ItemType.Gun,
            material = ShotgunMaterial
        };
        items["サブマシンガン"] = new Items
        {
            name = "サブマシンガン",
            num = SubMachineGun,
            damage = SubMachineGunDamage,
            fireRate = SubMachineGunFireRate,
            autoFire = SubMachineGunAutoFire,
            type = ItemType.Gun,
            material = SubMachineGunMaterial
        };
        items["アサルトライフル"] = new Items
        {
            name = "アサルトライフル",
            num = AssaultRifle,
            damage = AssaultRifleDamage,
            fireRate = AssaultRifleFireRate,
            autoFire = AssaultRifleAutoFire,
            type = ItemType.Gun,
            material = AssaultRifleMaterial
        };
        items["スナイパー"] = new Items
        {
            name = "スナイパー",
            num = SniperRifle,
            damage = SniperRifleDamage,
            fireRate = SniperRifleFireRate,
            autoFire = SniperRifleAutoFire,
            type = ItemType.Gun,
            material = SniperRifleMaterial
        };
        items["ライトマシンガン"] = new Items
        {
            name = "ライトマシンガン",
            num = LightMachineGun,
            damage = LightMachineGunDamage,
            fireRate = LightMachineGunFireRate,
            autoFire = LightMachineGunAutoFire,
            type = ItemType.Gun,
            material = LightMachineGunMaterial
        };
        items["鉄の弾"] = new Items
        {
            name = "鉄の弾",
            num = IronBullet,
            damage = IronBulletDamage,
            explosive = IronBulletIsExplosive,
            type = ItemType.Bullet,
            material = IronBulletMaterial,
            perCraft = IronBulletsPerCraft
        };
        items["炸裂弾"] = new Items
        {
            name = "炸裂弾",
            num = ExplosiveBullet,
            damage = ExplosiveBulletDamage,
            explosive = ExplosiveBulletIsExplosive,
            type = ItemType.Bullet,
            material = ExplosiveBulletMaterial,
            perCraft = ExplosiveBulletsPerCraft
        };
        items["ベイクドポテト"] = new Items
        {
            name = "ベイクドポテト",
            num = BakedPotato,
            heal = BakedPotatoHeal,
            type = ItemType.Food,
            material = BakedPotatoMaterial
        };
        items["シチュー"] = new Items
        {
            name = "シチュー",
            num = Stew,
            heal = StewHeal,
            type = ItemType.Food,
            material = StewMaterial
        };
        items["タレット"] = new Items
        {
            name = "タレット",
            num = Turret,
            type = ItemType.Turret,
            material = TurretMaterial
        };
    }
    //アイテム数を取得
    public int getItemNum(string name)
    {
        if (items.ContainsKey(name))
        {
            return items[name].num;
        }
        Debug.Log($"{name} は存在しません");
        return 0;
    }
    // アイテムのクラフト素材を取得
    public Dictionary<string, int> GetItemMaterial(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            return items[itemName].material ?? new Dictionary<string, int>();
        }
        Debug.Log($"{itemName} は存在しません");
        return new Dictionary<string, int>();
    }
    // アイテムの情報を取得
    public string getItemMessage(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.Log($"{name} は存在しません");
            return "";
        }
        var item = items[name];
        switch (item.type)
        {
            case ItemType.Gun:
                return $"敵を攻撃する武器。武器変更ボタンから切り替えることができる。";
            case ItemType.Bullet:
                return $"銃で敵を攻撃する際に消費する。";
            case ItemType.Food:
                return $"Hキーで使用する。体力を回復できる。";
            case ItemType.Turret:
                return $"設置すると自動で敵を攻撃する。フィールド上でFキーを2回押すと設置できる。";
            case ItemType.Material:
                return $"アイテムクラフト素材。";
            case ItemType.Money:
                return $"タワーの強化に使用する。";
            default:
                return $"";
        }
    }
    // アイテムの情報メッセージを取得
    public string getItemInfo(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.Log($"{name} は存在しません");
            return "";
        }
        var item = items[name];
        switch (item.type)
        {
            case ItemType.Gun:
                return $"ダメージ: {item.damage}\n連射力: {item.fireRate}\n所持数: {item.num}";
            case ItemType.Bullet:
                return $"ダメージ: {item.damage}\n所持数: {item.num}";
            case ItemType.Food:
                return $"回復量: {item.heal}\n所持数: {item.num}";
            case ItemType.Turret:
                return $"所持数: {item.num}";
            case ItemType.Material:
                return $"所持数: {item.num}";
            case ItemType.Money:
                return $"所持数: {item.num}";
            default:
                return $"所持数: {item.num}";
        }
    }
    // アイテム追加
    public void addItem(string name, int amount)
    {
        if (items.ContainsKey(name))
        {
            var item = items[name];
            item.num += amount;
            items[name] = item;
        }
        else
        {
            Debug.Log($"{name} は存在しません");
        }
    }
    // アイテム作成
    public void createItem(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.Log($"{name} は存在しません");
            return;
        }
        var item = items[name];
        var mats = item.material;
        // 素材チェック
        foreach (var kv in mats)
        {
            if (!items.ContainsKey(kv.Key) || items[kv.Key].num < kv.Value)
            {
                Debug.Log("ItemList.cs 材料が足りません");
                return;
            }
        }
        // 素材消費
        foreach (var kv in mats)
        {
            var mat = items[kv.Key];
            mat.num -= kv.Value;
            items[kv.Key] = mat;
        }
        // 作成
        item.num += (item.type == ItemType.Bullet ? item.perCraft : 1); // 弾薬は複数作成
        items[name] = item;
    }
    //アイテムドロップ処理
    public void dropItems(int waveNumber)
    {
        foreach (var itemName in DropItemName)
        {
            // 火薬は waveNumber >= 3 のときだけ
            if (itemName == "火薬" && waveNumber < 3) { continue; }
            // Items構造体からドロップ情報を取得
            if (items.ContainsKey(itemName))
            {
                var item = items[itemName];
                dropItem(itemName, item.dropNum, item.dropMin, item.dropMax);
            }
            else
            {
                Debug.Log($"{itemName}はItemListに登録されていません");
            }
        }
    }
    // 個別アイテムのドロップ処理
    private void dropItem(string name, float chance, int minAmount, int maxAmount)
    {
        // 確率を1以下に制限
        chance = Mathf.Min(chance, 1f);
        if (Random.value <= chance)
        {
            int amount = Random.Range(minAmount, maxAmount + 1);
            // コインのお守り補正（1個につき+1%）
            if (name == "コイン")
            {
                int coinCharm = PlayerPrefs.GetInt("Charm_Coin", 0);
                float bonus = 1f + (coinCharm * 0.005f);   // 0.5% × 個数
                amount = Mathf.RoundToInt(amount * bonus);
            }
            UIController.instance.AddToDropList(name, amount);
            addItem(name, amount);
        }
    }
    //回復アイテムの使用
    public bool useItem()
    {
        float currentHP = PlayerHealth.instance.getCurrentHP();
        float maxHP = PlayerHealth.instance.getMaxHP();
        // 候補リスト
        Items? bestItem = null;
        int bestHeal = 0;
        // 余分に回復しない候補
        foreach (var kv in items)
        {
            var item = kv.Value;
            if (item.type == ItemType.Food && item.num > 0)
            {
                if (currentHP + item.heal <= maxHP)
                {
                    // 余分なしならhealが大きいものを優先
                    if (item.heal > bestHeal)
                    {
                        bestHeal = item.heal;
                        bestItem = item;
                    }
                }
            }
        }
        // 余分なし候補がなければ、余分が最小のものを探す
        if (bestItem == null)
        {
            float minWaste = 999;
            foreach (var kv in items)
            {
                var item = kv.Value;
                if (item.type == ItemType.Food && item.num > 0)
                {
                    float waste = (currentHP + item.heal) - maxHP;
                    if (waste >= 0 && waste < minWaste)
                    {
                        minWaste = waste;
                        bestItem = item;
                    }
                }
            }
        }
        // アイテム使用
        if (bestItem != null)
        {
            var item = bestItem.Value;
            item.num--;
            items[item.name] = item;
            PlayerHealth.instance.Heal(item.heal);
            return true;
        }
        return false;
    }
    // 銃の所持数を返す
    public int getGunCount(string gunName)
    {
        return items.ContainsKey(gunName) ? items[gunName].num : 0;
    }
    // ダメージが最大の弾を探す
    private string FindBestBulletName()
    {
        int maxDamage = -1;
        string bestBulletName = null;
        foreach (var kv in items)
        {
            var item = kv.Value;
            if (item.type == ItemType.Bullet && item.num > 0)
            {
                if (item.damage > maxDamage)
                {
                    maxDamage = item.damage;
                    bestBulletName = kv.Key;
                }
            }
        }
        return bestBulletName;
    }
    // 弾使用
    public bool useBullet()
    {
        string bestBulletName = FindBestBulletName();
        if (bestBulletName != null)
        {
            var bullet = items[bestBulletName];
            currentBulletDamage = bullet.damage;
            isBulletExplosive = bullet.explosive;
            bullet.num--;
            items[bestBulletName] = bullet;
            return true;
        }
        else
        {
            currentBulletDamage = 0;
            isBulletExplosive = false;
            return false;
        }
    }
    // 銃切り替え
    public void switchGun(string gunName)
    {
        if (!items.ContainsKey(gunName)) { return; }
        var gun = items[gunName];
        if (gun.type == ItemType.Gun && gun.num > 0)
        {
            currentGun = gun.name;
            currentGunDamage = gun.damage;
            currentGunFireRate = gun.fireRate;
            currentGunAutoFire = gun.autoFire;
        }
        else
        {
            Debug.Log($"{gunName} を所持していません");
        }
    }
}