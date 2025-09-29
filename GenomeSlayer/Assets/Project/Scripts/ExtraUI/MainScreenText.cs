using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TextBlinker : MonoBehaviour
{
    private TextMeshProUGUI targetText; // TextMeshProUGUI를 쓰는 경우엔 Text 대신 TextMeshProUGUI로 변경
    public float blinkSpeed = 1.0f; // 깜빡이는 속도
    private Coroutine blinkCoroutine;

    private void Start()
    {
        targetText = GameObject.FindGameObjectWithTag("TitleText").GetComponent<TextMeshProUGUI>();
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    private void OnDestroy()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
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
        if (targetText == null) return;
        Color color = targetText.color;
        color.a = alpha;
        targetText.color = color;
    }
}