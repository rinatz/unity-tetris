using UnityEngine;

public class Tetromino : MonoBehaviour
{
    enum Status
    {
        Falling,            // 落下中
        Landing,            // 着地前
        PlacementLockdown,  // 着地前の遊びの時間
        Lockdown,           // 固定状態
    }

    public float defaultFallTime = 1.0f;
    private float fallTime;
    private float previousTime;
    private float accelerate = 20.0f;
    private Status status = Status.Falling;
    private TetriminoManager manager;

    void Start()
    {
        manager = FindObjectOfType<TetriminoManager>();
        fallTime = defaultFallTime;
        previousTime = Time.time;
    }

    void Update()
    {
        if (status == Status.Lockdown)
        {
            return;
        }

        if (Time.time - previousTime >= fallTime)
        {
            Fall();
            previousTime = Time.time;
        }

        HandleInput();

        if (status == Status.Landing)
        {
            status = Status.Lockdown;
            manager.OnTetrominoDropped(this);
        }
    }

    void Fall()
    {
        transform.position += Vector3.down;

        if (CheckCollision())
        {
            transform.position += Vector3.up;
            status = Status.Landing;
        }
    }

    void HandleInput()
    {
        // 着地前のキー入力は遊びの時間を設ける
        if (status == Status.Landing)
        {
            status = Status.PlacementLockdown;
        }

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
            fallTime = 0.0001f;
        }
        else
        {
            // 遊びの時間にキー入力がなければ着地前に戻す
            if (status == Status.PlacementLockdown)
            {
                status = Status.Landing;
            }
        }
    }

    bool CheckCollision()
    {
        return manager.CheckCollision(this);
    }
}
