using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // グリッドの幅
    public int width = 10;

    // グリッドの高さ
    public int height = 20;

    private Transform[,] grid;

    private int clearLineCount = 0;
    private int level = 1;

    public int Level
    {
        get { return level; }
    }

    private void Awake()
    {
        grid = new Transform[width, height];
    }

    public void Clear()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    continue;
                }

                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
    }

    public static Vector3 GetGridPosition(Vector3 p)
    {
        int x = Mathf.RoundToInt(p.x);
        int y = Mathf.RoundToInt(p.y);
        int z = Mathf.RoundToInt(p.z);

        return new Vector3(x, y, z);
    }

    public bool CheckCollision(Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (CheckCollision(child.transform.position))
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckCollision(Vector3 position)
    {
        var p = GetGridPosition(position);

        if (p.x < 0)
        {
            return true;
        }

        if (p.x >= width)
        {
            return true;
        }

        if (p.y < 0)
        {
            return true;
        }

        // y >= height のチェックはしない（テトリミノの生成後はこの条件を満たすため）

        if (p.x < width && p.y < height)
        {
            if (Filled(p))
            {
                return true;
            }
        }

        return false;
    }

    // テトリミノをグリッドに追加する
    public bool TryAdd(Transform transform)
    {
        foreach (Transform child in transform)
        {
            var p = GetGridPosition(child.transform.position);

            if (p.y >= height)
            {
                Debug.LogWarning("ミノが積み上がりました");
                return false;
            }

            grid[(int)p.x, (int)p.y] = child;
        }

        UpdateGrid();

        return true;
    }

    private void UpdateGrid()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            // y行がブロックで埋め尽くされたら削除する
            if (FilledLine(y))
            {
                ClearLine(y);
            }
        }
    }

    public bool Filled(Vector3 p)
    {
        return Filled((int)p.x, (int)p.y);
    }

    public bool Filled(int x, int y)
    {
        return grid[x, y] != null;
    }

    // y行はブロックで埋め尽くされたかどうかを調べる
    private bool FilledLine(int y)
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
    private void ClearLine(int y)
    {
        StartCoroutine(ClearLineCoroutine(y));
    }

    private IEnumerator ClearLineCoroutine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Debug.Log($"({x}, {y})を削除");

            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }

        PlayClearLineSound();

        yield return new WaitForSeconds(0.2f);

        FallOneRankAbove(y);

        clearLineCount++;
        UpdateLevel();
    }

    void UpdateLevel()
    {
        if (clearLineCount == 10)
        {
            Debug.LogWarning($"レベル{level}");

            level++;
            clearLineCount = 0;
        }
    }

    // yMin より上のブロックを下に移動させる
    private void FallOneRankAbove(int yMin)
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

    private void PlayClearLineSound()
    {
        GetComponent<AudioSource>().Play();
    }
}
