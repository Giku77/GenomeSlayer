using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateDef", menuName = "Scriptable Objects/StateDef")]
public class StateDef : ScriptableObject
{
    public Dictionary<int, int> states = new Dictionary<int, int>();
}
