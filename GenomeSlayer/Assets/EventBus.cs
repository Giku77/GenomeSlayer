using UnityEngine;
using System;

public static class EventBus
{
    public static Action EnemyDied;
    public static Action<Vector3> EnemyDropSeed;
    public static Action<int> WaveStarted;
    //public static Action<TreeEntity> TreeGrown;
    public static Action<int> PointsChanged;
}
