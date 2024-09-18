using System.Collections.Generic;
using UnityEngine;

public class TetriminoManager : MonoBehaviour
{
    public Vector3 spawnPosition;

    // テトリミノのプレハブを保存するリスト
    public List<GameObject> tetrominoList = new List<GameObject>();

    // シャッフルしたテトリミノのリスト
    private List<GameObject> currentTetrominoList = new List<GameObject>();

    // 次のテトリミノを保存するキュー
    private Queue<GameObject> nextTetrominoQueue = new Queue<GameObject>();

    // 表示用のスロット
    public List<Transform> nextTetrominoSlots;

    private const int Width = 10;
    private const int Height = 20;
    private Transform[,] grid = new Transform[Width, Height];

    void Start()
    {
        if (tetrominoList.Count == 0)
        {
            Debug.LogError("テトリミノのプレハブが設定されていません");
            return;
        }

        ShuffleCurrentTetrominoList();
        EnqueueNextTetrominoes();

        Debug.LogWarning("最初のミノを生成");

        SpawnTetromino();
    }

    public void SpawnTetromino()
    {
        var nextTetromino = nextTetrominoQueue.Dequeue();
        Instantiate(nextTetromino, spawnPosition, Quaternion.identity);

        EnqueueNextTetrominoes();
        UpdateNextTetrominoDisplay();
    }

    void ShuffleCurrentTetrominoList()
    {
        currentTetrominoList = new List<GameObject>(tetrominoList);

        for (int i = 0; i < currentTetrominoList.Count; i++)
        {
            int randomIndex = Random.Range(i, currentTetrominoList.Count);

            var temp = currentTetrominoList[i];
            currentTetrominoList[i] = currentTetrominoList[randomIndex];
            currentTetrominoList[randomIndex] = temp;
        }
    }

    void EnqueueNextTetrominoes()
    {
        // NEXTスロットに表示できる数だけキューに入れる
        while (nextTetrominoQueue.Count < nextTetrominoSlots.Count)
        {
            // リストが空なら再度シャッフル
            if (currentTetrominoList.Count == 0)
            {
                ShuffleCurrentTetrominoList();
            }

            nextTetrominoQueue.Enqueue(currentTetrominoList[0]);
            currentTetrominoList.RemoveAt(0);
        }
    }

    private void UpdateNextTetrominoDisplay()
    {
        // すでに表示されている次のミノを消す
        foreach (Transform slot in nextTetrominoSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // 次のミノを表示
        var nextTetrominoArray = nextTetrominoQueue.ToArray();

        for (int i = 0; i < nextTetrominoSlots.Count; i++)
        {
            if (i < nextTetrominoArray.Length)
            {
                var gameObject = Instantiate(nextTetrominoArray[i], nextTetrominoSlots[i]);

                gameObject.transform.localScale = Vector3.one * 0.5f;

                // 動かさないで表示させておく
                gameObject.GetComponent<Tetromino>().enabled = false;
            }
        }
    }

    public bool CheckCollision(Tetromino tetromino)
    {
        foreach (Transform child in tetromino.transform)
        {
            var x = Mathf.RoundToInt(child.transform.position.x);
            var y = Mathf.RoundToInt(child.transform.position.y);

            Debug.Log($"(x, y): ({x}, {y})");

            if (x < 0)
            {
                Debug.Log($"{child.name}が左に接触: ({x}, {y})");
                return true;
            }

            if (x >= Width)
            {
                Debug.Log($"{child.name}が右に接触: ({x}, {y})");
                return true;
            }

            if (y < 0)
            {
                Debug.Log($"{child.name}が地面に接触: ({x}, {y})");
                return true;
            }

            if (x < Width && y < Height)
            {
                if (HasTetromino(x, y))
                {
                    Debug.Log($"{child.name}が他のミノに接触: ({x}, {y})");
                    return true;
                }
            }
        }

        return false;
    }

    public void OnTetrominoLockdown(Tetromino tetromino)
    {
        if (!TryUpdateGrid(tetromino))
        {
            Debug.LogWarning("ゲームオーバー");
            return;
        }

        Debug.LogWarning($"{tetromino.gameObject.name}がロックダウン、次のミノを生成");

        SpawnTetromino();
    }

    bool HasTetromino(int x, int y)
    {
        return grid[x, y] != null;
    }

    bool TryUpdateGrid(Tetromino tetromino)
    {
        foreach (Transform child in tetromino.transform)
        {
            var x = Mathf.RoundToInt(child.transform.position.x);
            var y = Mathf.RoundToInt(child.transform.position.y);

            if (y >= Height)
            {
                Debug.LogWarning("ミノが積み上がりました");
                return false;
            }

            grid[x, y] = child;
        }

        for (int y = Height - 1; y >= 0; y--)
        {
            if (!CompletedLine(y))
            {
                continue;
            }

            ClearLine(y);
            FallOneRankAbove(y);
        }

        return true;
    }

    // y行はブロックで埋め尽くされたかどうかを調べる
    bool CompletedLine(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (!HasTetromino(x, y))
            {
                return false;
            }
        }

        Debug.Log($"{y}行目が削除可能");

        return true;
    }

    void ClearLine(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            Debug.Log($"({x}, {y})を削除");

            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    // yMin より上のブロックを下に移動させる
    public void FallOneRankAbove(int yMin)
    {
        for (int y = yMin + 1; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (!HasTetromino(x, y))
                {
                    continue;
                }

                grid[x, y - 1] = grid[x, y];
                grid[x, y - 1].transform.position += Vector3.down;

                grid[x, y] = null;
            }
        }
    }
}
