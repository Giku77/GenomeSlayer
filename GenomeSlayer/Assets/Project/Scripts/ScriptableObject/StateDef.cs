using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateDef", menuName = "Scriptable Objects/StateDef")]
public class StateDef : ScriptableObject
{
    public int GenomePoint;
    public int[] id;
    public int[] lv;
}
