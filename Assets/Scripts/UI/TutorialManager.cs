// MainScene TutorialCanvas/Panel
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [SerializeField] private GameObject tutorialPanel;     // チュートリアル表示パネル
    [SerializeField] private TextMeshProUGUI tutorialText; // チュートリアル文章表示

    private int currentIndex = 0;                          // 現在のメッセージ番号
    private bool isTutorialActive = false;                 // チュートリアル中かどうか

    // 各チュートリアルの完了フラグ
    private bool isPreparationTutorialComplete = false;
    private bool isTowerTutorialComplete = false;
    private bool isWaveTutorialComplete = false;

    // 現在どのチュートリアルを進行中か
    private bool isPreparationTutorial = false;
    private bool isTowerTutorial = false;
    private bool isWaveTutorial = false;

    // 準備時間チュートリアルのメッセージ
    private string[] preparationMessages = new string[]
    {
        "これからチュートリアルを始めます。",
        "まずは画面の説明をします。",
        "画面上部中央にはタワーのHP、画面下部中央にはプレイヤーのHPが表示されています。",
        "画面左上には現在のWave、右上には準備時間の残り時間が表示されます。",
        "ゲームは「準備時間」と敵の出現する「Wave」の繰り返しです。",
        "ゲームを始めると、まず準備時間が始まります。",
        "準備時間はタワーやプレイヤーの強化、アイテムのクラフトなどを行います。",
        "移動はWASDキー、視点移動はマウス操作、ジャンプはスペースキーで行います。",
        "Iキーでインベントリの表示、Tabキーでメニューの表示、Hキーで回復アイテムの使用ができます。",
        "タレットを持っている場合、タワーから離れてFキーを押すとタレットを設置できます。",
        "準備時間中にタワーの近くでFキーを押すことで、タワーメニューが表示されます。",
    };

    // タワーチュートリアルのメッセージ
    private string[] TowerMessages = new string[]
    {
        "これがタワーメニューです。",
        "アップグレードボタンを押すと、タワー強化画面が表示されます。",
        "敵がドロップするコインを使ってタワーを強化できます。",
        "クラフトボタンを押すと、アイテムクラフト画面が表示されます。",
        "敵がドロップする素材アイテムを使って、様々なアイテムをクラフトできます。",
        "武器変更ボタンを押すと、武器変更画面が表示されます。",
        "ここで所持している武器に変更できます。",
        "ミニゲームボタンを押すと、ミニゲーム画面が表示されます。",
        "ミニゲームを行うことでプレイヤーのステータスを強化できます。",
        "Waveスタートボタンを押すと、準備時間が終了しWaveが開始されます。",
    };

    // Waveチュートリアルのメッセージ
    private string[] WaveMessages = new string[]
    {
        "Waveが始まりました。",
        "Wave中は、右クリックで視点切り替え、左クリックで弾の発射ができます。",
        "敵が出現するので敵に照準を合わせて弾を発射し、敵を倒してください。",
        "敵を倒すとアイテムやスコアが入手できます。",
        "連続で倒すとより多くのスコアを入手できます。",
        "プレイヤーに対して近接攻撃する敵や、タワーに対して近接攻撃する敵、プレイヤーに対して遠距離攻撃する敵が出現します。",
        "プレイヤーのHPが0になると、一定時間操作不能になったあとに復活します。",
        "タワーのHPが0になるとゲームオーバーです。",
        "一定数の敵を倒すとWaveクリアになります。",
        "また、難易度ごとに決められたWaveをクリアするとゲームクリアになります。",
        "ゲームクリアまたはゲームオーバー時にクリアしたWave数に応じてアクセサリーを入手できます。",
        "チュートリアルは以上です。"
    };

    private void Awake()
    {
        instance = this;
        tutorialPanel.SetActive(false); // 初期は非表示
    }

    // 準備時間チュートリアル開始
    public void StartPreparationTutorial()
    {
        if (isPreparationTutorialComplete) { return; }
        isTutorialActive = true;
        isPreparationTutorial = true;
        currentIndex = 0;
        tutorialPanel.SetActive(true);
        tutorialText.text = preparationMessages[currentIndex];
    }
    // タワーチュートリアル開始
    public void StartTowerTutorial()
    {
        if (isTowerTutorialComplete) { return; }
        isTutorialActive = true;
        isTowerTutorial = true;
        currentIndex = 0;
        tutorialPanel.SetActive(true);
        tutorialText.text = TowerMessages[currentIndex];
        // タワーUIの操作を一時的に無効化
        TowerUIManager.instance.SetInteractable(false);
    }
    // Waveチュートリアル開始
    public void StartWaveTutorial()
    {
        if (isWaveTutorialComplete) { return; }
        isTutorialActive = true;
        isWaveTutorial = true;
        currentIndex = 0;
        tutorialPanel.SetActive(true);
        tutorialText.text = WaveMessages[currentIndex];
    }
    // 次のメッセージへ進む
    public void nextMessage()
    {
        if (!isTutorialActive) { return; }
        currentIndex++;
        // 準備チュートリアル
        if (isPreparationTutorial)
        {
            if (currentIndex >= preparationMessages.Length)
            {
                isPreparationTutorialComplete = true;
                isPreparationTutorial = false;
                tutorialPanel.SetActive(false);
                isTutorialActive = false;
                return;
            }
            tutorialText.text = preparationMessages[currentIndex];
        }
        // タワーチュートリアル
        else if (isTowerTutorial)
        {
            if (currentIndex >= TowerMessages.Length)
            {
                isTowerTutorialComplete = true;
                isTowerTutorial = false;
                tutorialPanel.SetActive(false);
                isTutorialActive = false;
                // タワーUIの操作を再度有効化
                TowerUIManager.instance.SetInteractable(true);
                return;
            }
            tutorialText.text = TowerMessages[currentIndex];
        }
        // Waveチュートリアル
        else if (isWaveTutorial)
        {
            if (currentIndex >= WaveMessages.Length)
            {
                isWaveTutorialComplete = true;
                isWaveTutorial = false;
                tutorialPanel.SetActive(false);
                isTutorialActive = false;
                return;
            }
            tutorialText.text = WaveMessages[currentIndex];
        }
    }
    // 現在チュートリアル中かどうか
    public bool getTutorialActive()
    {
        return isTutorialActive;
    }
}