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

    // 着してから固定されるまでの遊びの時間
    public float landingTime = 0.3f;

    // 落下間隔
    private float fallTime;

    // 前回落下した時間
    private float previousTime;

    private Status status = Status.Standby;

    public GameObject ghostBlock;
    private GameObject ghostBlockObject;

    public AudioClip moveSound;
    public AudioClip rotationSound;
    public AudioClip lockdownSound;

    GridManager GridManager
    {
        get
        {
            return FindObjectOfType<GridManager>();
        }
    }

    void Start()
    {
        fallTime = defaultFallTime;
        previousTime = Time.time;
    }

    void Update()
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

        // 着地したら終了
        if (status == Status.Lockdown)
        {
            GetComponent<AudioSource>().PlayOneShot(lockdownSound);
            Destroy(ghostBlockObject);
            FindObjectOfType<TetrominoManager>().Lockdown(this);
        }
    }

    bool Controllable()
    {
        return status == Status.Falling || status == Status.Landing;
    }

    public void Falling()
    {
        status = Status.Falling;
        ghostBlockObject = Instantiate(ghostBlock);
        UpdateGhostBlock();
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
        if (GridManager.CheckCollision(transform))
        {
            transform.position += Vector3.up;

            if (status == Status.Falling)
            {
                status = Status.Landing;

                // 固定するまで遊びの時間を設ける
                fallTime = landingTime;
            }
            else if (status == Status.Landing)
            {
                status = Status.Lockdown;
            }
        }
    }

    void HandleInput()
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
                GetComponent<AudioSource>().PlayOneShot(moveSound);
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
                GetComponent<AudioSource>().PlayOneShot(moveSound);
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
                GetComponent<AudioSource>().PlayOneShot(rotationSound);
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

    void UpdateGhostBlock()
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
}
