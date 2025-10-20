using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public AudioClip AudioClip;

    private void Awake()
    {
        #if UNITY_EDITOR
                Debug.unityLogger.logEnabled = true;
        #else
                Debug.unityLogger.logEnabled = false;
        #endif
    }
    private void Start()
    {
         AudioManager.I.PlayBGM(AudioClip);
    }
    public void ChangeScene(string sceneName)
    {
        AudioManager.I.PlaySFX("mainButton");
        SceneLoader.I.Load(sceneName);
        //SceneManager.LoadScene(sceneName);
    }
}