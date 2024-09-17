using UnityEngine;

public class TetriminoManager : MonoBehaviour
{
    public GameObject[] tetrominoList;
    private int currentIndex = 0;
    private const int Width = 10;
    private const int Height = 20;
    private Transform[,] grid = new Transform[Width, Height];

    void Start()
    {
        Debug.LogWarning("最初のミノを生成");
        SpawnTetromino();
    }

    public GameObject SpawnTetromino()
    {
        if (currentIndex == 0)
        {
            ShuffleTetrominoList();
        }

        var gameObject = Instantiate(tetrominoList[currentIndex++], transform.position, Quaternion.identity);

        if (currentIndex >= tetrominoList.Length)
        {
            currentIndex = 0;
        }

        return gameObject;
    }

    public void ShuffleTetrominoList()
    {
        for (int i = tetrominoList.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            var temp = tetrominoList[i];
            tetrominoList[i] = tetrominoList[j];
            tetrominoList[j] = temp;
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

            if (x < grid.GetLength(0) && y < grid.GetLength(1))
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
