using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoFactory : MonoBehaviour
{
    // テトリミノの生成位置
    public Vector3 spawnPosition;

    // テトリミノのプレハブを保存するリスト
    public List<GameObject> tetrominoList = new List<GameObject>();

    // シャッフルしたテトリミノのリスト
    private List<GameObject> currentTetrominoList = new List<GameObject>();

    // 次のテトリミノを保存するキュー
    private Queue<GameObject> nextTetrominoQueue = new Queue<GameObject>();

    // 表示用のスロット
    public List<Transform> nextTetrominoSlots;

    void Awake()
    {
        if (tetrominoList.Count == 0)
        {
            Debug.LogError("テトリミノのプレハブが設定されていません");
            return;
        }

        if (nextTetrominoSlots.Count == 0)
        {
            Debug.LogError("次のテトリミノを表示するスロットが設定されていません");
            return;
        }

        ShuffleCurrentTetrominoList();
        EnqueueNextTetrominoList();
    }

    public GameObject Spawn()
    {
        var nextTetromino = nextTetrominoQueue.Dequeue();
        var gameObject = Instantiate(nextTetromino, spawnPosition, Quaternion.identity);
        gameObject.GetComponent<Tetromino>().Falling();

        EnqueueNextTetrominoList();
        UpdateNextTetrominoDisplay();

        return gameObject;
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

    void EnqueueNextTetrominoList()
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

    void UpdateNextTetrominoDisplay()
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
            }
        }
    }
}
