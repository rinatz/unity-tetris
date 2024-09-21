using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoFactory : MonoBehaviour
{
    // テトリミノの生成位置
    public Vector3 spawnPosition;

    // テトリミノのプレハブを保存するリスト
    public List<GameObject> tetrominoList = new List<GameObject>();

    // テトリミノを取り出し順に格納したキュー
    private Queue<GameObject> tetrominoQueue = new Queue<GameObject>();

    // 表示用のスロット
    public List<Transform> nextSlots;

    void Awake()
    {
        if (tetrominoList.Count == 0)
        {
            Debug.LogError("テトリミノのプレハブが設定されていません");
            return;
        }

        if (nextSlots.Count == 0)
        {
            Debug.LogError("次のテトリミノを表示するスロットが設定されていません");
            return;
        }

        Enqueue(Shuffle(tetrominoList));
    }

    public GameObject Spawn()
    {
        // 次のテトリミノを取り出す
        var gameObject = Dequeue();

        // 表示用のテトリミノがスロット数に満たない場合はテトリミノをキューに追加
        while (tetrominoQueue.Count < nextSlots.Count)
        {
            Enqueue(Shuffle(tetrominoList));
        }

        // 次のテトリミノを表示
        DisplayNext();

        return gameObject;
    }

    public void Clear()
    {
        ClearNextSlots();
        ClearTetrominoQueue();
    }

    void ClearNextSlots()
    {
        foreach (Transform slot in nextSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void ClearTetrominoQueue()
    {
        tetrominoQueue.Clear();
        Enqueue(Shuffle(tetrominoList));
    }

    static List<GameObject> Shuffle(List<GameObject> items)
    {
        var shuffled = new List<GameObject>(items);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);

            var temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled;
    }

    void Enqueue(List<GameObject> items)
    {
        foreach (var item in items)
        {
            tetrominoQueue.Enqueue(item);
        }
    }

    GameObject Dequeue()
    {
        var next = tetrominoQueue.Dequeue();

        var gameObject = Instantiate(next, spawnPosition, Quaternion.identity);
        gameObject.GetComponent<Tetromino>().Falling();

        return next;
    }

    void DisplayNext()
    {
        // すでに表示されている次のミノを消す
        ClearNextSlots();

        // 次のミノを表示
        var nextTetrominoArray = tetrominoQueue.ToArray();

        for (int i = 0; i < nextSlots.Count; i++)
        {
            var gameObject = Instantiate(nextTetrominoArray[i], nextSlots[i]);
            gameObject.transform.localScale = Vector3.one * 0.5f;
        }
    }
}
