using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TextBlinker : MonoBehaviour
{
    public TextMeshProUGUI targetText; // TextMeshProUGUI를 쓰는 경우엔 Text 대신 TextMeshProUGUI로 변경
    public float blinkSpeed = 1.0f; // 깜빡이는 속도

    private void Start()
    {
        StartCoroutine(BlinkText());
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            // 알파값을 0 → 1 → 0으로 반복
            for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime * blinkSpeed)
            {
                SetAlpha(alpha);
                yield return null;
            }

            for (float alpha = 1f; alpha >= 0f; alpha -= Time.deltaTime * blinkSpeed)
            {
                SetAlpha(alpha);
                yield return null;
            }
        }
    }

    void SetAlpha(float alpha)
    {
        Color color = targetText.color;
        color.a = alpha;
        targetText.color = color;
    }
}