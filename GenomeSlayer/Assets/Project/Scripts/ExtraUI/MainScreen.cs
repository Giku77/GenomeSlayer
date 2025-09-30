using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public AudioClip AudioClip;
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