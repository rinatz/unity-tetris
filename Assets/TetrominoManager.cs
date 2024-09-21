using System.Collections;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    public GameObject playText;
    public GameObject gameOverText;
    public AudioClip playSound;
    private GridManager grid;
    private TetrominoFactory factory;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        factory = FindObjectOfType<TetrominoFactory>();
    }

    private void Update()
    {
        if (!Ready())
        {
            return;
        }

        StartCoroutine(PlayCoroutine());
    }

    private bool Ready()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            return true;
        }

        return false;
    }

    private IEnumerator PlayCoroutine()
    {
        Debug.Log("プレイ開始");

        GetComponent<AudioSource>().PlayOneShot(playSound);

        yield return new WaitForSeconds(1.0f);

        Destroy(playText);
        PlaySound();

        factory.Spawn();
    }

    private void GameOver()
    {
        Debug.LogWarning("ゲームオーバー");

        Instantiate(gameOverText);
    }

    void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }

    // テトリミノの落下位置を記録して新たなテトリミノを生成
    public void Lockdown(Tetromino tetromino)
    {
        Debug.LogWarning($"{tetromino.gameObject.name}がロックダウン");

        if (!grid.TryAdd(tetromino.transform))
        {
            GameOver();
            return;
        }

        Debug.LogWarning("次のミノを生成");

        factory.Spawn();
    }
}
