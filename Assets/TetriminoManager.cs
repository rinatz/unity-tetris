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

    private Grid grid;

    void Start()
    {
        if (tetrominoList.Count == 0)
        {
            Debug.LogError("テトリミノのプレハブが設定されていません");
            return;
        }

        grid = FindObjectOfType<Grid>();

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
        return grid.CheckCollision(tetromino);
    }

    public void OnLockdown(Tetromino tetromino)
    {
        if (!grid.TryAdd(tetromino))
        {
            Debug.LogWarning("ゲームオーバー");
            return;
        }

        Debug.LogWarning($"{tetromino.gameObject.name}がロックダウン、次のミノを生成");

        SpawnTetromino();
    }
}
