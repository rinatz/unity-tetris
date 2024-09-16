using UnityEngine;

public class Tetromino : MonoBehaviour
{
    public float defaultFallTime = 1.0f;
    private float fallTime;
    private float previousTime;
    private float accelerate = 20.0f;
    private bool lockdown = false;
    private TetriminoManager manager;

    void Start()
    {
        manager = FindObjectOfType<TetriminoManager>();
        fallTime = defaultFallTime;
        previousTime = Time.time;
    }

    void Update()
    {
        if (lockdown)
        {
            return;
        }

        if (Time.time - previousTime >= fallTime)
        {
            Fall();
            previousTime = Time.time;
        }

        HandleInput();

        if (lockdown)
        {
            manager.OnTetrominoDropped(this);
        }
    }

    void Fall()
    {
        transform.position += Vector3.down;

        if (CheckCollision())
        {
            transform.position += Vector3.up;
            lockdown = true;
        }
    }

    bool HardDropMode()
    {
        return Input.GetKeyDown(KeyCode.Space);
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
            fallTime = 0.0001f;
        }
    }

    bool CheckCollision()
    {
        return manager.CheckCollision(this);
    }
}
