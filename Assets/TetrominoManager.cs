using System.Collections;
using TMPro;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    public GameObject playText;
    public GameObject gameOverText;
    public AudioClip okButtonSound;
    private GridManager grid;
    private TetrominoFactory factory;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        factory = FindObjectOfType<TetrominoFactory>();

        StartCoroutine(FlushOkButtonText(0.5f));
    }

    private void Update()
    {
        if (!Ready())
        {
            return;
        }

        StartCoroutine(PlayCoroutine());
    }

    private IEnumerator FlushOkButtonText(float seconds)
    {
        var child = playText.transform.Find("PressOkButtonText");

        if (child != null)
        {
            var text = child.GetComponent<TextMeshProUGUI>();

            while (true)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
                yield return new WaitForSeconds(seconds);

                text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
                yield return new WaitForSeconds(seconds);
            }
        }
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

        GetComponent<AudioSource>().PlayOneShot(okButtonSound);
        StartCoroutine(FlushOkButtonText(0.05f));

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
