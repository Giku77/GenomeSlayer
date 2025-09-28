using System;

[Serializable]
public class SettingsSaveData
{
    public int version = 1;

    public int tutorial = 1;
    public int showFPS;
    public int vibration = 1;

    public float lookSensitivity = 0.32f;   
    public float invertY = 0.1f;
    public float deadZone = 0.1f;        

    public float bgmVol = 1f;
    public float sfxVol = 1f;

    public int qualityLevel = 2;          
}
