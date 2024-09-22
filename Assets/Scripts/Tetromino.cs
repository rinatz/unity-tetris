using UnityEngine;

public class Tetromino : MonoBehaviour
{
    private enum Status
    {
        Standby,    // 投下待ち
        Falling,    // 落下中
        Holding,    // ホールド中
        Landing,    // 着地中
        Lockdown,   // ロックダウン（固定）
    }

    // テトリミノのステータス
    private Status status = Status.Standby;

    // ホールドできるかどうか
    private bool canHold = true;

    // 落下間隔（レベルごとにデフォルトが変化）
    public float defaultFallTime = 1.0f;

    // ソフトドロップの加速倍率
    public float accelerate = 20.0f;

    // ハードドロップの間隔
    public float hardDropTime = 0.0001f;

    // 着してから固定されるまでの遊びの時間
    public float landingTime = 0.3f;

    // 落下間隔を測るタイマー
    private Timer fallTimer;

    // ロックダウンが発生するまでの時間を測るタイマー
    private Timer lockdownTimer;

    // ゴーストブロック（落下位置を示すブロック）のプレハブ
    public GameObject ghostBlock;

    // 生成されたゴーストブロック
    private GameObject ghostBlockObject;

    // 移動時の効果音
    public AudioClip moveSound;

    // 回転時の効果音
    public AudioClip rotationSound;

    // ロックダウン時の効果音
    public AudioClip lockdownSound;

    private GridManager GridManager
    {
        get
        {
            return FindObjectOfType<GridManager>();
        }
    }

    private TetrominoManager TetrominoManager
    {
        get
        {
            return FindObjectOfType<TetrominoManager>();
        }
    }

    private AudioSource AudioSource
    {
        get
        {
            return GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        defaultFallTime = 1.0f / GridManager.level;
        fallTimer = new Timer(defaultFallTime);
        lockdownTimer = new Timer(0.5f);
    }

    private void Update()
    {
        // 操作が無効の場合は何もしない
        if (!Controllable())
        {
            return;
        }

        Debug.Log($"{transform.name} - status: {status}");

        // 着地したら status が変化する
        Fall();
        HandleInput();

        // ゴーストブロックを更新
        UpdateGhostBlock();

        if (status == Status.Landing)
        {
            // 着地して遊びの時間が過ぎたらロックダウン
            if (lockdownTimer.Elapsed)
            {
                status = Status.Lockdown;
                Lockdown();
            }
        }
    }

    private bool Controllable()
    {
        return status == Status.Falling || status == Status.Landing;
    }

    public void Falling()
    {
        // ホールドから戻ってきたときはレベルが上る前の可能性があるためここで更新
        if (status == Status.Holding)
        {
            defaultFallTime = 1.0f / GridManager.level;
            fallTimer = new Timer(defaultFallTime);
        }

        status = Status.Falling;

        ghostBlockObject = Instantiate(ghostBlock);
        UpdateGhostBlock();
    }

    private void Fall()
    {
        // 落下時間になるまで待機する
        if (!fallTimer.Elapsed)
        {
            return;
        }

        transform.position += Vector3.down;

        // 範囲外に行ったら1マス戻して無効化する（着地）
        if (GridManager.CheckCollision(transform))
        {
            transform.position += Vector3.up;

            if (status == Status.Falling)
            {
                status = Status.Landing;

                // 固定するまで遊びの時間を設ける
                lockdownTimer.Reset();
            }
        }

        // タイマーをリセットして次のフレームへ
        fallTimer.Reset();
    }

    private void HandleInput()
    {
        // 左移動（左ボタン）
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;

            if (GridManager.CheckCollision(transform))
            {
                transform.position += Vector3.right;
            }
            else
            {
                PlayMoveSound();
            }

            // 着地中に操作したら落下中に戻す
            if (status == Status.Landing)
            {
                status = Status.Falling;
            }
        }
        // 右移動（右ボタン）
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;

            if (GridManager.CheckCollision(transform))
            {
                transform.position += Vector3.left;
            }
            else
            {
                PlayMoveSound();
            }

            // 着地中に操作したら落下中に戻す
            if (status == Status.Landing)
            {
                status = Status.Falling;
            }
        }
        // 回転（上ボタン）
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var angle = -90;

            transform.Rotate(Vector3.forward, angle);

            if (GridManager.CheckCollision(transform))
            {
                transform.Rotate(Vector3.forward, -angle);
            }
            else
            {
                PlayRotationSound();
            }

            // 着地中に操作したら落下中に戻す
            if (status == Status.Landing)
            {
                status = Status.Falling;
            }
        }
        // ホールド
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Hold();
        }
        // ソフトドロップ
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Fall();
        }
        // ソフトドロップ（高速）
        if (Input.GetKey(KeyCode.DownArrow))
        {
            fallTimer.interval = defaultFallTime / accelerate;
        }
        // ソフトドロップ（高速）の解除
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            fallTimer.interval = defaultFallTime;
        }
        // ハードドロップ
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            fallTimer.interval = hardDropTime;
        }
    }

    private void UpdateGhostBlock()
    {
        var position = transform.position;
        bool validPosition = true;
        int step = 1;

        while (validPosition)
        {
            foreach (Transform child in transform)
            {
                var p = child.transform.position - new Vector3(0, step, 0);

                if (GridManager.CheckCollision(p))
                {
                    validPosition = false;
                    position.y -= step - 1;
                    break;
                }
            }

            step++;
        }

        ghostBlockObject.transform.position = position;
        ghostBlockObject.transform.rotation = transform.rotation;
    }

    private void Lockdown()
    {
        PlayLockdownSound();
        Destroy(ghostBlockObject);

        TetrominoManager.Lockdown(this);
    }

    private void Hold()
    {
        if (!canHold)
        {
            return;
        }

        // ホールドは一度しかできない
        canHold = false;

        status = Status.Holding;
        Destroy(ghostBlockObject);

        TetrominoManager.Hold(this);
    }

    private void PlayMoveSound()
    {
        AudioSource.PlayOneShot(moveSound);
    }

    private void PlayRotationSound()
    {
        AudioSource.PlayOneShot(rotationSound);
    }

    private void PlayLockdownSound()
    {
        AudioSource.PlayOneShot(lockdownSound);
    }
}
