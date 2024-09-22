using System.Collections;
using TMPro;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI subText;
    public AudioClip startSound;
    private GridManager grid;
    private TetrominoFactory factory;
    private float flushTime;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        factory = FindObjectOfType<TetrominoFactory>();

        mainText.text = "PLAY";
        subText.text = "PRESS START";
    }

    private void Update()
    {
        flushTime += Time.deltaTime * 5.0f;

        var color = subText.color;
        color.a = Mathf.Sin(flushTime);

        subText.color = color;

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

        mainText.text = "";
        subText.text = "";
        GetComponent<AudioSource>().PlayOneShot(startSound);

        yield return new WaitForSeconds(1.0f);

        PlaySound();
        factory.Spawn();
    }

    private IEnumerator GameOverCoroutine()
    {
        Debug.LogWarning("ゲームオーバー");

        mainText.text = "GAMEOVER";

        yield return new WaitForSeconds(3.0f);

        GetComponent<AudioSource>().Stop();
        grid.Clear();
        factory.Clear();

        mainText.text = "PLAY";
        subText.text = "PRESS START";
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

            StartCoroutine(GameOverCoroutine());

            return;
        }

        Debug.LogWarning("次のミノを生成");

        factory.Spawn();
    }

    public void Hold(Tetromino tetromino)
    {
        factory.Hold(tetromino);
    }
}
