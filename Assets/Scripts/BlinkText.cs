using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;
    private float time;
    private float speed = 5.0f;

    private void Start()
    {
        textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        time += Time.deltaTime * speed;

        var color = textMeshProUGUI.color;

        color.a = Mathf.Sin(time);
        textMeshProUGUI.color = color;
    }

    public void Blink(string text)
    {
        Debug.Log($"{text}");

        speed = 5.0f;
        textMeshProUGUI.text = text;
    }

    public void BlinkFast(string text)
    {
        speed = 50.0f;
        textMeshProUGUI.text = text;
    }
}
