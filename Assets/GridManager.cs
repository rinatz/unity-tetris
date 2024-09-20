using UnityEngine;

public class GridManager : MonoBehaviour
{
    // グリッドの幅
    public int width = 10;

    // グリッドの高さ
    public int height = 20;

    private Transform[,] grid;

    void Awake()
    {
        grid = new Transform[width, height];
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

            if (x >= width)
            {
                Debug.Log($"{child.name}が右に接触: ({x}, {y})");
                return true;
            }

            if (y < 0)
            {
                Debug.Log($"{child.name}が地面に接触: ({x}, {y})");
                return true;
            }

            if (x < width && y < height)
            {
                if (Filled(x, y))
                {
                    Debug.Log($"{child.name}が他のミノに接触: ({x}, {y})");
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckCollision(int x, int y)
    {
        Debug.Log($"(x, y): ({x}, {y})");

        if (x < 0)
        {
            Debug.Log($"左に接触: ({x}, {y})");
            return true;
        }

        if (x >= width)
        {
            Debug.Log($"右に接触: ({x}, {y})");
            return true;
        }

        if (y < 0)
        {
            Debug.Log($"地面に接触: ({x}, {y})");
            return true;
        }

        if (x < width && y < height)
        {
            if (Filled(x, y))
            {
                Debug.Log($"他のミノに接触: ({x}, {y})");
                return true;
            }
        }

        return false;
    }

    // テトリミノをグリッドに追加する
    public bool TryAdd(Tetromino tetromino)
    {
        foreach (Transform child in tetromino.transform)
        {
            var x = Mathf.RoundToInt(child.transform.position.x);
            var y = Mathf.RoundToInt(child.transform.position.y);

            if (y >= height)
            {
                Debug.LogWarning("ミノが積み上がりました");
                return false;
            }

            grid[x, y] = child;
        }

        UpdateGrid();

        return true;
    }

    void UpdateGrid()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            if (!FilledLine(y))
            {
                continue;
            }

            ClearLine(y);
        }
    }

    public bool Filled(int x, int y)
    {
        return grid[x, y] != null;
    }

    // y行はブロックで埋め尽くされたかどうかを調べる
    bool FilledLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (!Filled(x, y))
            {
                return false;
            }
        }

        Debug.Log($"{y}行目が埋め尽くされた");

        return true;
    }

    // y 行目を削除する
    void ClearLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Debug.Log($"({x}, {y})を削除");

            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }

        FallOneRankAbove(y);
    }

    // yMin より上のブロックを下に移動させる
    void FallOneRankAbove(int yMin)
    {
        for (int y = yMin + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
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
