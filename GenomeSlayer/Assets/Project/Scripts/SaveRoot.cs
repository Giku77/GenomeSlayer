using System;

[Serializable]
public class SaveRoot
{
    public int version = 1;
    public string savedAtIso;          
    public StateSaveData state;
    public WavesSaveData waves;
}

[System.Serializable]
public class WavesSaveData
{
    public int nextWaveIndex;   
    public bool inProgress;   
}
