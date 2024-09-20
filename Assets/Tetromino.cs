using UnityEngine;

public class Tetromino : MonoBehaviour
{
    enum Status
    {
        Standby,
        Falling,
        Landing,
        Lockdown,
    }

    // 落下間隔（デフォルト）
    public float defaultFallTime = 1.0f;

    // ソフトドロップの加速倍率
    public float accelerate = 20.0f;

    // ハードドロップの間隔
    public float hardDropTime = 0.0001f;

    // 落下間隔
    private float fallTime;

    // 前回落下した時間
    private float previousTime;

    // テトリミノの管理クラス
    private TetrominoManager manager;

    private Status status = Status.Standby;

    void Start()
    {
        fallTime = defaultFallTime;
        previousTime = Time.time;
        manager = FindObjectOfType<TetrominoManager>();
    }

    void Update()
    {
        // 操作が無効の場合は何もしない
        if (status != Status.Falling)
        {
            return;
        }

        // 着地したら enabled フラグが落ちる
        Fall();
        HandleInput();

        // 着地したら終了
        if (status == Status.Landing)
        {
            manager.Lockdown(this);
            status = Status.Lockdown;
        }
    }

    public void Falling()
    {
        status = Status.Falling;
    }

    void Fall()
    {
        // fallTime 以上経過するまで待機する
        if (Time.time - previousTime < fallTime)
        {
            return;
        }

        previousTime = Time.time;
        transform.position += Vector3.down;

        // 範囲外に行ったら1マス戻して無効化する（着地）
        if (CheckCollision())
        {
            transform.position += Vector3.up;
            status = Status.Landing;
        }
    }

    void HandleInput()
    {
        // 左移動（左ボタン）
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;

            if (CheckCollision())
            {
                transform.position += Vector3.right;
            }
        }
        // 右移動（右ボタン）
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;

            if (CheckCollision())
            {
                transform.position += Vector3.left;
            }
        }
        // 回転（上ボタン）
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var angle = -90;

            transform.Rotate(Vector3.forward, angle);

            if (CheckCollision())
            {
                transform.Rotate(Vector3.forward, -angle);
            }
        }
        // ソフトドロップ
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Fall();
        }
        // ソフトドロップ（高速）
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            fallTime = defaultFallTime / accelerate;
        }
        // ソフトドロップ（高速）の解除
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            fallTime = defaultFallTime;
        }
        // ハードドロップ
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            fallTime = hardDropTime;
        }
    }

    bool CheckCollision()
    {
        return manager.CheckCollision(this);
    }
}
