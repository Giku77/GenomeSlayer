using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public static class EventBus
{
    public static List<GameObject> RemoveObj = new List<GameObject>();

    //추후 이벤트형식으로 변경해서 안정성 챙길것
    public static Action<GameObject> EnemyDied;
    public static Action<Vector3> EnemyDropSeed;
    public static Action<int> WaveStarted;
    //public static Action<TreeEntity> TreeGrown;
    public static Action<int> PointsChanged;
    public static Action<int, string, string> UpdateSlot;
    public static Action RaiseFruitHarvested;
}
