//MainScene Player
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Transform _transform;                     // プレイヤー自身のTransform
    private CharacterController _characterController; // 移動処理を担当
    private Animator _animator;                       // アニメーション制御
    public static PlayerController instance;
    private bool canControl = true;                   // プレイヤー操作の有効/無効
    [SerializeField] private Button firstButtonOnSettingPanel; // 設定画面の初期選択ボタン
    [SerializeField] private Button firstButtonOnTowerPanel; // タワー画面の初期選択ボタン

    //入力関連
    private Vector2 _inputMove; //移動入力
    private Vector2 _inputLook; //視点入力 
    private float _lookSpeed = 0.1f; //視点の感度
    private float _verticalRotation = 0f; //上下方向の感度
    //移動関連
    private float _verticalVelocity; //垂直方向の速度
    private bool _isGroundedPrev; //前フレームで着地していたか
    [SerializeField] private float _speed = 8; //移動速度
    [SerializeField] private float _jumpSpeed = 8; //ジャンプ初速
    [SerializeField] private float _gravity = 20; //重力加速度
    [SerializeField] private float _fallSpeed = 20; //落下速度の上限
    [SerializeField] private float _initFallSpeed = 10; //着地直後の落下速度
    //射撃関連
    [SerializeField] private float bulletSpeed = 500f; //弾速
    [SerializeField] private float bulletLifetime = 5f; //弾の寿命
    private bool isFiring = false; // オート射撃中か
    private float fireTimer = 0f; // 発射間隔管理
    //タレット関連
    private bool isPlacingTurret = false; // タレット設置モード中か
    private GameObject turretGhost; // 設置位置のゴースト表示
    private float placementTimer = 0f; // ゴースト生成後の経過時間
    //食事関連
    [SerializeField] private float eatCoolDown = 1f; // 食事のクールダウン
    private float lastEatTime = -999f; // 最後に食べた時間
    [Header("プレハブ・カメラ")]
    [SerializeField] private Camera playerCamera;      // プレイヤー視点カメラ
    [SerializeField] private Transform shootPoint;     // 弾の発射位置
    [SerializeField] private GameObject bulletPrefab;  // 弾プレハブ
    [SerializeField] private GameObject turretPrefab;  // タレット本体
    [SerializeField] private GameObject turretGhostPrefab; // タレット設置ゴースト
    [SerializeField] private LayerMask groundLayer;    // 地面判定用レイヤー

    [Header("スコープ設定")]
    [SerializeField] private float scopedFOV = 10f;    // スコープ時の視野角
    private float normalFOV;                           // 通常時の視野角
    [SerializeField] private Vector3 scopedCameraPosition = new Vector3(0.3f, 1.8f, 1f); // スコープ時のカメラ位置
    [SerializeField] private GameObject scopeOverlayUI; // スコープUI
    [SerializeField] private GameObject crosshairUI;    // クロスヘアUI
    private Vector3 normalCameraPosition;               // 通常時のカメラ位置
    private bool isScoped = false;                      // スコープ状態かどうか
    private bool isTowerOpen = false;                   // タワーUIが開いているかどうか
    private string difficulty;                          // 難易度    
    private void Awake()
    {
        _transform = transform;
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        instance = this;
    }
    // ゲーム開始時の初期設定
    private void Start()
    {
        difficulty = MainManager.instance != null ? MainManager.instance.getDifficulty() : "Easy"; // 難易度取得
        Cursor.lockState = CursorLockMode.Locked; //マウスカーソルを画面中央に固定する
        Cursor.visible = false; //マウスカーソルを非表示にする
        // カメラ設定
        normalFOV = playerCamera.fieldOfView;
        normalCameraPosition = playerCamera.transform.localPosition;
        scopeOverlayUI.SetActive(false);
        // 感度設定
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.1f);
        SetLookSpeed(sensitivity);
        // 参照チェック
        if (playerCamera == null)
        {
            Debug.LogWarning("PlayerController.cs: playerCameraが未設定です");
        }
        if (shootPoint == null)
        {
            Debug.LogWarning("PlayerController.cs: shootPointが未設定です");
        }
        if (bulletPrefab == null)
        {
            Debug.LogWarning("PlayerController.cs: bulletPrefabが未設定です");
        }
        if (ItemList.instance == null)
        {
            Debug.LogWarning("PlayerController.cs: ItemList.instanceがnullです");
        }
        // プレイヤーの移動速度補正
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            _speed = _speed * health.getMovementSpeedParse();
        }
        else
        {
            Debug.LogWarning("PlayerController.cs: PlayerHealthが見つかりません");
        }
    }
    // 視点感度の設定
    public void SetLookSpeed(float value)
    {
        _lookSpeed = value;
    }
    // プレイヤー操作の有効/無効
    public void isControlled(bool control)
    {
        canControl = control;
        if (control)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    // 現在の移動速度を取得
    public float getMovementSpeed()
    {
        return _speed;
    }
    // タワーUIが開いているかどうかを取得
    public bool getIsTowerOpen()
    {
        return isTowerOpen;
    }
    // 射撃入力
    public void OnFire(InputAction.CallbackContext context)
    {
        // チュートリアル中はクリックで次へ進む
        if (TutorialManager.instance.getTutorialActive())
        {
            if (context.performed)
            {
                TutorialManager.instance.nextMessage();
            }
            return;
        }
        // 装備中の銃がオートの場合(Update内で発射処理を行う)
        if (ItemList.instance.isAutoFire())
        {
            if (context.performed)
            {
                isFiring = true; // 押した瞬間から発射開始
            }
            else if (context.canceled)
            {
                isFiring = false; // 離したら停止
            }
        }
        // セミオートの場合
        else
        {
            isFiring = false;
            if (context.performed)
            {
                // Wave中のみ射撃可能
                if (fireTimer <= 0f && EnemySpawner.instance.getCurrentState() == EnemySpawner.GameState.Wave)
                {
                    FireBullet();
                    fireTimer = ItemList.instance.getFireRate();
                }
            }
        }
    }
    // 移動入力
    public void OnMove(InputAction.CallbackContext context)
    {
        _inputMove = context.ReadValue<Vector2>();
    }
    // ジャンプ入力
    public void OnJump(InputAction.CallbackContext context)
    {
        // ボタンが押された瞬間かつ着地している時だけ処理
        if (!context.performed || !_characterController.isGrounded)
        {
            return;
        }
        // 上向きに速度を与える
        _verticalVelocity = _jumpSpeed;
    }
    // 視点入力
    public void OnLook(InputAction.CallbackContext context)
    {
        _inputLook = context.ReadValue<Vector2>();
    }
    // インベントリ入力
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (TutorialManager.instance.getTutorialActive()) { return; }
        if (isTowerOpen) { return; }
        if (context.performed)
        {
            if (InventoryUI.instance == null)
            {
                return;
            }
            //インベントリ画面を開く
            InventoryUI.instance.ToggleInventory();
        }
    }
    // 設定画面入力
    public void OnSetting(InputAction.CallbackContext context)
    {
        if (TutorialManager.instance.getTutorialActive()) { return; }
        if (isTowerOpen) { return; }
        if (context.performed)
        {
            if (SettingManager.instance == null) { return; }
            //設定画面を開く
            EventSystem.current.SetSelectedGameObject(firstButtonOnSettingPanel.gameObject);
            SettingManager.instance.ToggleSetting();
        }
    }
    // インタラクト入力
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (TutorialManager.instance.getTutorialActive()) { return; }
        if (!context.performed) { return; }
        if (TowerUIManager.instance == null) { return; }
        if (isTowerOpen) { return; }
        // Wave中は操作できないようにする
        if (EnemySpawner.instance.getCurrentState() == EnemySpawner.GameState.Wave) { return; }
        GameObject tower = GameObject.FindGameObjectWithTag("Tower");
        if (tower == null)
        {
            Debug.LogWarning("PlayerController.cs Towerタグのオブジェクトが見つかりません");
            return;
        }
        // プレイヤーとタワーの距離を測定
        float distance = Vector3.Distance(transform.position, tower.transform.position);
        // 近ければタワーUIを開く
        if (distance <= 15f)
        {
            isTowerOpen = true;
            EventSystem.current.SetSelectedGameObject(firstButtonOnTowerPanel.gameObject);
            TowerUIManager.instance.ToggleUI();
            if (difficulty == "Tutorial") { TutorialManager.instance.StartTowerTutorial(); }
        }
        // 遠ければタレット設置処理をする
        else if (!isPlacingTurret && ItemList.instance.getItemNum("タレット") > 0)
        {
            EnterPlacementMode(); //タレットを仮置き
        }
        else if (isPlacingTurret)
        {
            ConfirmPlacement(); //タレットを設置
        }
    }
    // タワーUIを閉じた時に呼ばれる
    public void towerClose()
    {
        isTowerOpen = false;
    }
    // タレットを設置できる位置かどうか判定
    private bool CanPlaceTurret(Vector3 position)
    {
        float turretMinDistance = 3f; // 最低距離（これより近いと設置不可）
        return !TurretManager.instance.IsTooClose(position, turretMinDistance);
    }
    // タレット設置モードに入る
    void EnterPlacementMode()
    {
        isPlacingTurret = true; // タレット設置モードにする
        placementTimer = 0f;
        // プレイヤー前方3mを基準位置にする
        Vector3 spawnPos = transform.position + transform.forward * 3f;
        // 地面チェック
        if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            Vector3 pos = hit.point;
            // タレット重複チェック
            if (!CanPlaceTurret(pos))
            {
                Debug.Log("ここにはタレットを設置できません");
                isPlacingTurret = false; // 設置モード解除
                return;
            }
            // 設置可能な場合のみゴースト生成
            turretGhost = Instantiate(turretGhostPrefab, pos, Quaternion.identity);
        }
    }
    // タレット設置を確定する
    void ConfirmPlacement()
    {
        //ゴースト設置から0.2秒以上経過しているならタレットを設置
        if (turretGhost != null && placementTimer >= 0.2f)
        {
            Vector3 pos = turretGhost.transform.position;
            // タレット本体を生成
            Instantiate(turretPrefab, turretGhost.transform.position, turretGhost.transform.rotation);
            TurretManager.instance.RegisterTurret(pos); // TurretManagerに登録
            Destroy(turretGhost); // ゴースト削除
            turretGhost = null;
            isPlacingTurret = false; // 設置モード終了
            ItemList.instance.addItem("タレット", -1); // タレットを1つ消費
            AudioManager.instance.PlaySound(AudioManager.instance.audioClips.decisionSound, 0.1f);
        }
    }
    // スコープ入力
    public void OnScope(InputAction.CallbackContext context)
    {
        if (TutorialManager.instance.getTutorialActive()) { return; }
        isScoped = context.performed;
        // スコープUI切り替え
        if (ItemList.instance.getGunName() == "スナイパー")
        {
            scopeOverlayUI.SetActive(isScoped);
            crosshairUI.SetActive(!isScoped);
        }
    }
    // 回復アイテム入力
    public void OnEat(InputAction.CallbackContext context)
    {
        if (TutorialManager.instance.getTutorialActive()) { return; }
        if (isTowerOpen) { return; }
        // クールダウン中は使用不可
        if (Time.time - lastEatTime < eatCoolDown) { return; }
        if (ItemList.instance != null)
        {
            // アイテム使用成功時のみ処理
            if (ItemList.instance.useItem())
            {
                lastEatTime = Time.time;
                AudioManager.instance.PlaySoundRandomPitch(AudioManager.instance.audioClips.eatSound, 0.7f, 0.98f, 1.02f);
            }
        }
    }
    // 弾を発射する
    private void FireBullet()
    {
        // 射撃アニメーション
        if (_animator != null)
        {
            _animator.SetTrigger("IsFiring");
        }
        // 弾が無い場合は射撃不可
        if (ItemList.instance.useBullet())
        {
            // ショットガン以外
            if (ItemList.instance.getGunName() != "ショットガン")
            {
                // 弾を生成
                GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                Vector3 direction = playerCamera.transform.forward;
                rb.linearVelocity = direction * bulletSpeed;
                Bullet bulletComp = bullet.GetComponent<Bullet>();
                bulletComp.setPlayerBullet();
                Destroy(bullet, bulletLifetime);
                if (AudioManager.instance != null && AudioManager.instance.audioClips != null)
                {
                    AudioManager.instance.PlaySoundRandomPitch(AudioManager.instance.audioClips.shootSound, 0.05f, 0.98f, 1.02f);
                }
            }
            // ショットガン
            else
            {
                int pelletCount = 8; //生成する弾の個数
                float spreadAngle = 5f; //弾の拡散角度
                for (int i = 0; i < pelletCount; i++)
                {
                    //プレーヤーの視点方向のランダムな角度を設定
                    Quaternion spreadRotation = Quaternion.Euler(
                        Random.Range(-spreadAngle, spreadAngle),
                        Random.Range(-spreadAngle, spreadAngle),
                        0f
                    );
                    Vector3 spreadDirection = spreadRotation * playerCamera.transform.forward;
                    //弾を生成
                    GameObject pellet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(spreadDirection));
                    Rigidbody rb = pellet.GetComponent<Rigidbody>();
                    rb.linearVelocity = spreadDirection * bulletSpeed;
                    Bullet bulletComp = pellet.GetComponent<Bullet>();
                    bulletComp.setPlayerBullet();
                    Destroy(pellet, bulletLifetime);
                }
                if (AudioManager.instance != null && AudioManager.instance.audioClips != null)
                {
                    AudioManager.instance.PlaySoundRandomPitch(AudioManager.instance.audioClips.shootSound, 0.05f, 0.98f, 1.02f);
                }
            }
        }
        else
        {
            Debug.Log("PlayerController.cs 弾がありません");
        }
    }
    private void Update()
    {
        if (playerCamera == null || shootPoint == null || bulletPrefab == null || ItemList.instance == null || _characterController == null || !_characterController.enabled)
        {
            return;
        }
        if (!canControl) { return; } // UI中は操作しない
        var isGrounded = _characterController.isGrounded;
        // 視点切り替え処理
        if (ItemList.instance.getGunName() == "スナイパー")
        {
            float targetFOV = isScoped ? scopedFOV : normalFOV;
            playerCamera.fieldOfView = targetFOV;
        }
        Vector3 targetPos = isScoped ? scopedCameraPosition : normalCameraPosition;
        playerCamera.transform.localPosition = targetPos;
        // タレット設置中にプレイヤーとゴーストの距離が離れすぎたら設置をキャンセルする
        if (isPlacingTurret)
        {
            placementTimer += Time.deltaTime;
            if (turretGhost != null)
            {
                float distance = Vector3.Distance(transform.position, turretGhost.transform.position);
                if (distance > 10f)
                {
                    Destroy(turretGhost);
                    turretGhost = null;
                    isPlacingTurret = false;
                }
            }
        }
        // オート武器の射撃処理
        if (EnemySpawner.instance.getCurrentState() == EnemySpawner.GameState.Wave)
        {
            if (fireTimer > 0f)
            {
                fireTimer -= Time.deltaTime;
            }
            if (isFiring)
            {
                if (fireTimer <= 0f)
                {
                    FireBullet();
                    fireTimer = ItemList.instance.getFireRate();
                }
            }
        }
        // キャラクターの落下処理
        if (isGrounded && !_isGroundedPrev)// 前フレームでは空中で今フレームで地面に接した
        {
            _verticalVelocity = -_initFallSpeed; // 着地時の速度を設定
        }
        else if (!isGrounded) // 空中にいる
        {
            // 下向きに重力加速度を与えて落下させる
            _verticalVelocity -= _gravity * Time.deltaTime;
            // 速度上限を設定
            if (_verticalVelocity < -_fallSpeed)
            {
                _verticalVelocity = -_fallSpeed;
            }
        }
        _isGroundedPrev = isGrounded;
        // カメラの上下回転
        _verticalRotation -= _inputLook.y * _lookSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        // プレイヤーの左右回転
        _transform.Rotate(Vector3.up * _inputLook.x * _lookSpeed);
        // プレイヤーの移動
        Vector3 moveDirection = _transform.forward * _inputMove.y + _transform.right * _inputMove.x;
        Vector3 moveVelocity = moveDirection * _speed;
        moveVelocity.y = _verticalVelocity;
        _characterController.Move(moveVelocity * Time.deltaTime);
        // アニメーション
        if (_animator != null)
        {
            float speed = new Vector2(_inputMove.x, _inputMove.y).magnitude;
            _animator.SetFloat("Speed", speed);
        }
    }
}