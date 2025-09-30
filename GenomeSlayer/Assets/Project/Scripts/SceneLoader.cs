using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader I;
    [Header("Assign a prefab with Canvas/Progress UI")]
    public GameObject loadingCanvasPrefab;

    GameObject loadingCanvas;
    Image spinner;
    TextMeshProUGUI progressText;      

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Load(string sceneName)
    {
        StartCoroutine(CoLoad(sceneName));
    }

    IEnumerator CoLoad(string sceneName)
    {
        if (loadingCanvas == null)
        {
            loadingCanvas = Instantiate(loadingCanvasPrefab);
            DontDestroyOnLoad(loadingCanvas);

            spinner = loadingCanvas.GetComponentsInChildren<Image>(true)[1];
            progressText = loadingCanvas.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        loadingCanvas.SetActive(true);

        yield return null;

        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float display = 0f;                 
        const float speed = 1.0f;            
        const float minShowTime = 0.3f;     
        float t = 0f;

        while (!op.isDone)
        {
            float target = (op.progress < 0.9f) ? (op.progress / 0.9f) : 1f;

            display = Mathf.MoveTowards(display, target, speed * Time.unscaledDeltaTime);
            UpdateUI(display);

            t += Time.unscaledDeltaTime;

            if (display >= 0.999f && op.progress >= 0.9f && t >= minShowTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        yield return new WaitForEndOfFrame();
        loadingCanvas.SetActive(false);
    }

    void UpdateUI(float p)  // p: 0~1
    {
        Debug.Log($"Loading progress: {p}");
        if (spinner) spinner.rectTransform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime);
        //if (progressText) progressText.text = $"·ÎµùÁß {Mathf.RoundToInt(p * 100f)}%";
    }
}
