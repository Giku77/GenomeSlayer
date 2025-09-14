using UnityEngine;
using System;

public static class EventBus
{
    //추후 이벤트형식으로 변경해서 안정성 챙길것
    public static Action EnemyDied;
    public static Action<Vector3> EnemyDropSeed;
    public static Action<int> WaveStarted;
    //public static Action<TreeEntity> TreeGrown;
    public static Action<int> PointsChanged;
}
