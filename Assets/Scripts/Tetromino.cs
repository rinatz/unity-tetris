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
    public float defaultFallInterval = 1.0f;

    // ソフトドロップの加速倍率
    public float softDropAcceleration = 20.0f;

    // ハードドロップの間隔
    public float hardDropInterval = 0.0001f;

    // 着してから固定されるまでの遊びの時間
    public float landingInterval = 0.3f;

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

    // 回転時のウォールキックのオフセット表
    private Vector3[] wallKickOffsets = new Vector3[] {
        Vector3.zero,       // 回転そのまま
        Vector3.left,       // 左に1マス
        Vector3.right,      // 右に1マス
        Vector3.up,         // 上に1マス
        Vector3.down,       // 下に1マス
        Vector3.left * 2,   // 左に2マス
        Vector3.right * 2,  // 右に2マス
        Vector3.up * 2,     // 上に2マス
        Vector3.down * 2,   // 下に2マス
    };

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
        defaultFallInterval = 1.0f / GridManager.level;
        fallTimer = new Timer(defaultFallInterval);
        lockdownTimer = new Timer(0.5f);
    }

    private void Update()
    {
        // 操作が無効の場合は何もしない
        if (!Controllable())
        {
            return;
        }

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
                Debug.Log($"{transform.name}がロックダウン");

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
            defaultFallInterval = 1.0f / GridManager.level;
            fallTimer = new Timer(defaultFallInterval);
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
                Debug.Log($"{transform.name}がプレイスメントロックダウン");

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

            // 衝突したら未操作と同じ扱い
            if (GridManager.CheckCollision(transform))
            {
                transform.position += Vector3.right;
                return;
            }

            PlayMoveSound();

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

            // 衝突したら未操作と同じ扱い
            if (GridManager.CheckCollision(transform))
            {
                transform.position += Vector3.left;
                return;
            }

            PlayMoveSound();

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

            if (!TryWallKick())
            {
                transform.Rotate(Vector3.forward, -angle);
                return;
            }

            PlayRotationSound();

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

            // 着地中にソフトドロップしたら遊びの時間をなくす
            if (status == Status.Landing)
            {
                lockdownTimer.interval = 0f;
            }
        }
        // ソフトドロップ（高速）
        if (Input.GetKey(KeyCode.DownArrow))
        {
            fallTimer.interval = defaultFallInterval / softDropAcceleration;

            // 着地中にソフトドロップしたら遊びの時間をなくす
            if (status == Status.Landing)
            {
                lockdownTimer.interval = 0f;
            }
        }
        // ソフトドロップ（高速）の解除
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            fallTimer.interval = defaultFallInterval;
        }
        // ハードドロップ
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            fallTimer.interval = hardDropInterval;
        }
    }

    // 壁に衝突しないように位置をずらしながらなるべく回転させる（ウォールキック）
    private bool TryWallKick()
    {
        foreach (var offset in wallKickOffsets)
        {
            // ブロックの位置をオフセットさせる
            transform.position += offset;

            // その位置が有効かどうかを確認する
            if (!GridManager.CheckCollision(transform))
            {
                // 有効なら回転を確定
                return true;
            }

            // オフセットが無効だった場合、元の位置に戻す
            transform.position -= offset;
        }

        return false;
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
