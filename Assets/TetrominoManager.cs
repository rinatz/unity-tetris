using System.Collections;
using TMPro;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    public GameObject playText;
    private GameObject playTextObject;
    public GameObject gameOverText;
    public AudioClip okButtonSound;
    private GridManager grid;
    private TetrominoFactory factory;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        factory = FindObjectOfType<TetrominoFactory>();

        playTextObject = Instantiate(playText);
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
        var child = playTextObject.transform.Find("PressOkButtonText");

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

        Destroy(playTextObject);
        PlaySound();

        factory.Spawn();
    }

    private IEnumerator GameOver()
    {
        Debug.LogWarning("ゲームオーバー");

        var gameObject = Instantiate(gameOverText);

        yield return new WaitForSeconds(3.0f);

        GetComponent<AudioSource>().Stop();
        Destroy(gameObject);
        grid.Clear();
        factory.Clear();

        playTextObject = Instantiate(playText);
        StartCoroutine(FlushOkButtonText(0.5f));
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
            // グリッドからはみ出た分はグリッドの削除で一緒に削除できないのでここで削除
            Destroy(tetromino.gameObject);

            StartCoroutine(GameOver());
            return;
        }

        Debug.LogWarning("次のミノを生成");

        factory.Spawn();
    }

    public bool TryHold(Tetromino tetromino)
    {
        return factory.TryHold(tetromino);
    }
}
