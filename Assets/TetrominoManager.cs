using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    private GridManager grid;
    private TetrominoFactory factory;

    void Start()
    {
        grid = FindObjectOfType<GridManager>();
        factory = FindObjectOfType<TetrominoFactory>();

        Debug.LogWarning("最初のミノを生成");

        factory.Spawn();
    }

    // テトリミノの落下位置を記録して新たなテトリミノを生成
    public void Lockdown(Tetromino tetromino)
    {
        Debug.LogWarning($"{tetromino.gameObject.name}がロックダウン");

        if (!grid.TryAdd(tetromino.transform))
        {
            Debug.LogWarning("ゲームオーバー");
            return;
        }

        Debug.LogWarning("次のミノを生成");

        factory.Spawn();
    }
}
