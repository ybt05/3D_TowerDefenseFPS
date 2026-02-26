//MainScene Manager/EnemySpawnerObject
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
/// 敵のスポーンとゲーム進行を管理する
public class EnemySpawner : MonoBehaviour
{
    // ゲームの状態
    public enum GameState
    {
        Preparation, // 準備時間
        Wave,        // 敵が出現するWave中
        GameClear    // 全Wave終了
    }
    // Waveごとの設定データ
    [System.Serializable]
    public class Wave
    {
        public int waveNumber;   // Wave番号
        public int enemyCount;   // 敵の数
        public float spawnInterval; // 敵の出現間隔
        public float enemyHP;    // 敵のHP
        public float enemyDamage;// 敵の攻撃力
    }
    [Header("Wave設定")]
    [SerializeField] private Wave[] waves;              // 各Waveの設定
    [SerializeField] private GameObject enemyPrefab;    // 生成する敵のプレハブ
    [SerializeField] private Transform tower;           // 敵が向かうタワーの位置
    [SerializeField] private float spawnRadius = 60f;   // 敵をスポーンさせる半径

    [SerializeField] private float prepareTime = 60f;   // 準備時間

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText; // 残り準備時間の表示
    [SerializeField] private TextMeshProUGUI waveText;      // 現在のWave表示
    [SerializeField] private Button startWaveButton;        // Wave開始ボタン
    private float hpMultiplier = 1f;          // 敵HP倍率
    private float damageMultiplier = 1f;      // 敵攻撃力倍率
    private float countMultiplier = 1f;       // 敵出現数倍率
    private float prepareTimeMultiplier = 1f; // 準備時間倍率
    private int currentWave = 0;              // 現在のWave番号
    private static GameState CurrentState;    // ゲームの状態
    private bool isReadyPressed = false;      // プレイヤーが「Wave開始」を押したか
    private bool isQuizExplanationOpen = false; // クイズ解説中かどうか
    private float timer;                      // 準備時間・Wave時間のカウントダウン用
    private string difficulty;                // 選択された難易度
    public static EnemySpawner instance;
    private void Start()
    {
        ApplyDifficultyModifiers(); // 難易度に応じて敵や準備時間の倍率を適用
        instance = this;
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(false);
            startWaveButton.onClick.AddListener(OnStartWaveButtonPressed);
        }
        StartCoroutine(GameFlow()); // ゲーム進行コルーチン開始
    }
    public GameState getCurrentState()
    {
        return CurrentState;
    }
    public void QuizExplanation(bool isOpen)
    {
        isQuizExplanationOpen = isOpen;
    }
    // 難易度に応じて敵や準備時間の倍率を適用
    private void ApplyDifficultyModifiers()
    {
        difficulty = MainManager.instance != null ? MainManager.instance.getDifficulty() : "Easy"; // 難易度を取得
        switch (difficulty)
        {
            case "Tutorial":
                hpMultiplier = 0.5f;
                damageMultiplier = 0.2f;
                countMultiplier = 1f;
                prepareTimeMultiplier = 2f;
                break;
            case "Easy":
                hpMultiplier = 1f;
                damageMultiplier = 1f;
                countMultiplier = 1f;
                prepareTimeMultiplier = 1f;
                break;
            case "Hard":
                hpMultiplier = 2f;
                damageMultiplier = 1.5f;
                countMultiplier = 1f;
                prepareTimeMultiplier = 2f;
                break;
            default: // Normal
                hpMultiplier = 1.5f;
                damageMultiplier = 1.2f;
                countMultiplier = 1f;
                prepareTimeMultiplier = 1.5f;
                break;
        }
        // 時間のお守り補正（1個につき +1%）
        int timeCharmCount = PlayerPrefs.GetInt("Charm_Time", 0);
        float timeCharmBonus = 1f + (timeCharmCount * 0.01f);
        // 準備時間に難易度補正+お守り補正を適用
        prepareTime *= prepareTimeMultiplier;
        prepareTime *= timeCharmBonus;

        // wavesに倍率を適用
        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].enemyHP *= hpMultiplier;
            waves[i].enemyDamage *= damageMultiplier;
            waves[i].enemyCount = Mathf.CeilToInt(waves[i].enemyCount * countMultiplier);
        }
    }
    /// ゲーム全体の進行を管理する
    private IEnumerator GameFlow()
    {
        // Waveがすべて終わるまで繰り返す
        for (int i = 0; i < waves.Length; i++)
        {
            currentWave = i + 1;
            if (MainManager.instance != null)
            {
                MainManager.instance.changeWave(currentWave);
            }
            // 準備タイム
            CurrentState = GameState.Preparation;
            yield return StartCoroutine(PreparationTime());
            // Wave開始
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.startSound, 0.2f);
            AudioManager.instance.PlayBGMWithFade(AudioManager.instance.audioClips.bgmSound, 1.5f);
            yield return StartCoroutine(SpawnWave(waves[i]));
            AudioManager.instance.StopBGMWithFade(1.0f);
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.endSound, 0.2f);
            // 難易度ごとのWave数設定
            if (difficulty == "Tutorial" && i == 2)
            {
                currentWave++;
                if (MainManager.instance != null)
                {
                    MainManager.instance.changeWave(currentWave);
                }
                break;
            }
            if (difficulty == "Easy" && i == 2)
            {
                currentWave++;
                if (MainManager.instance != null)
                {
                    MainManager.instance.changeWave(currentWave);
                }
                break;
            }
            else if (difficulty == "Normal" && i == 4)
            {
                currentWave++;
                if (MainManager.instance != null)
                {
                    MainManager.instance.changeWave(currentWave);
                }
                break;
            }
        }
        // 全Wave終了
        CurrentState = GameState.GameClear;
        countdownText.text = "";
        waveText.text = "";
        // 3秒待ってからGameClearを呼ぶ
        yield return new WaitForSeconds(3f);
        GameClear();
    }
    public void GameClear()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameClearScene");
    }
    // クイズ不正解で準備時間を減らす処理
    public void TimerControl()
    {
        if (timer > 0)
        {
            if (difficulty == "Hard")
            {
                timer -= 10f;
            }
            else if (difficulty == "Normal")
            {
                timer -= 3;
            }
            if (timer < 0.01f)
            {
                timer = 0.01f;
            }
        }
        if (countdownText != null)
        {
            countdownText.text = $"Wave {currentWave} まで: {Mathf.Ceil(timer)}s";
        }
    }
    // 準備時間の管理
    private IEnumerator PreparationTime()
    {
        timer = prepareTime;
        isReadyPressed = false;
        // Wave開始ボタンを有効化
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(true);
            startWaveButton.interactable = true;
        }
        waveText.text = $"Wave {currentWave}";
        if (difficulty == "Tutorial") { TutorialManager.instance.StartPreparationTutorial(); }
        // タイマーが0になるか、プレイヤーが開始ボタンを押すまで待機
        while (timer > 0 && !isReadyPressed)
        {
            // クイズ説明中・チュートリアル中は時間停止
            if (!isQuizExplanationOpen && !TutorialManager.instance.getTutorialActive())
            {
                timer -= Time.deltaTime;
            }
            countdownText.text = $"Wave {currentWave} まで: {Mathf.Ceil(timer)}s";
            yield return null;
        }
        CurrentState = GameState.Wave;
        // UIを閉じる
        if (TowerUIManager.instance != null)
        {
            TowerUIManager.instance.CloseUI();
        }
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(false);
        }
        countdownText.text = $"Wave {currentWave} スタート";
        yield return new WaitForSeconds(2f);
    }
    // Wave中の敵スポーン処理
    private IEnumerator SpawnWave(Wave wave)
    {
        countdownText.text = "";
        if (difficulty == "Tutorial") { TutorialManager.instance.StartWaveTutorial(); }
        // チュートリアルが終わるまで待機
        while (TutorialManager.instance.getTutorialActive()) { yield return null; }
        // 敵を順番にスポーン
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave.enemyHP, wave.enemyDamage);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
        // 全ての敵が倒れるまで待機
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return null;
        }
        // Wave終了表示
        countdownText.text = $"Wave {currentWave} クリア";
        yield return new WaitForSeconds(2f);
        countdownText.text = "";
    }
    // 敵をランダムな位置にスポーンさせる
    private void SpawnEnemy(float waveHP, float waveDamage)
    {
        float angleRad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = tower.position.x + Mathf.Cos(angleRad) * spawnRadius;
        float z = tower.position.z + Mathf.Sin(angleRad) * spawnRadius;
        Vector3 spawnPos = new Vector3(x, 2f, z);
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(currentWave, waveHP, waveDamage);
        }
    }
    // Wave開始ボタンが押された時の処理
    public void OnStartWaveButtonPressed()
    {
        if (CurrentState != GameState.Preparation) { return; }
        isReadyPressed = true;
        if (startWaveButton != null)
        {
            startWaveButton.interactable = false;
        }
    }
}
