using UnityEngine;


public enum BuffType
{
    None = -1,
    buff1 = 1411001,
    buff2 = 1412002,
    buff3 = 1414003,
    buff4 = 1423004,
    buff5 = 1434005,
}
public class Buff : MonoBehaviour
{
    public BuffType buffType;
    public int buffCount;

    public Buff(BuffType type, int count)
    {
            buffType = type;
            buffCount = count;
    }

    private void Start()
    {
        
    }

    public void ApplyBuff()
    {
       switch (buffType)
       {
            case BuffType.buff1:
                Debug.Log("Applying Buff 1");
                
                break;
            case BuffType.buff2:
                Debug.Log("Applying Buff 2");
                
                break;
            case BuffType.buff3:
                Debug.Log("Applying Buff 3");
                
                break;
            case BuffType.buff4:
                Debug.Log("Applying Buff 4");
                
                break;
            case BuffType.buff5:
                Debug.Log("Applying Buff 5");
               
                break;
            default:
                Debug.LogWarning("Unknown Buff Type");
                break;
        }
    }
}
