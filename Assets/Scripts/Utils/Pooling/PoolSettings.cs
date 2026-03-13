using UnityEngine;

[System.Serializable]
public class PoolSettings
{
    [Min(0)] public int InitialSize = 8;
    [Min(1)] public int MaxSize = 128;
    public bool AutoExpand = true;
}
