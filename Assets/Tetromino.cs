using UnityEngine;

public class Tetromino : MonoBehaviour
{
    public float dropSpeed = 1.0f;
    public float accelerationFactor = 30.0f;
    private float lapTime;
    private bool dropped = false;
    private TetriminoManager manager;

    void Start()
    {
        manager = FindObjectOfType<TetriminoManager>();
        lapTime = NextLapTime();
    }

    void Update()
    {
        if (dropped)
        {
            return;
        }

        if (Time.time >= lapTime)
        {
            Drop();
            lapTime = NextLapTime();
        }

        HandleInput();
    }

    void Drop()
    {
        transform.position += Vector3.down;

        if (CheckCollision())
        {
            transform.position += Vector3.up;

            dropped = true;
            manager.OnTetrominoDropped(this);
        }
    }

    bool IsFastDropMode()
    {
        return Input.GetKey(KeyCode.DownArrow);
    }

    float NextLapTime()
    {
        if (IsFastDropMode())
        {
            return Time.time + dropSpeed / accelerationFactor;
        }

        return Time.time + dropSpeed;
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
        // 下移動（下ボタン）
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Drop();
        }
    }

    bool CheckCollision()
    {
        return manager.CheckCollision(this);
    }
}
