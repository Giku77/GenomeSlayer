using System.Collections;
using TMPro;
using UnityEngine;

public class TypingText : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    [Range(1, 120)] public float charsPerSecond = 30f;
    public bool pauseOnPunct = true;           // 문장부호에서 잠깐 쉬기
    public float punctPause = 0.2f;            // 쉼표/마침표 지연
    public KeyCode skipKey = KeyCode.Space;    // 스킵 키

    Coroutine routine;
    bool skipping;

    public void Play(string message)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeRoutine(message));
    }

    IEnumerator TypeRoutine(string message)
    {
        skipping = false;
        tmp.text = message;
        tmp.ForceMeshUpdate();                         // 글자 수 계산 준비
        int total = tmp.textInfo.characterCount;       // 가시 문자 수
        tmp.maxVisibleCharacters = 0;

        float tPerChar = 1f / Mathf.Max(1f, charsPerSecond);
        int i = 0;

        while (i < total)
        {
            if (Input.GetKeyDown(skipKey)) skipping = true;

            if (skipping)
            {
                tmp.maxVisibleCharacters = total;      
                break;
            }

            i++;
            tmp.maxVisibleCharacters = i;

            if (pauseOnPunct)
            {
                var ci = tmp.textInfo.characterInfo[i - 1];
                char c = ci.character;
                if (c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':')
                    yield return new WaitForSeconds(punctPause);
            }

            yield return new WaitForSeconds(tPerChar);
        }
    }
}
