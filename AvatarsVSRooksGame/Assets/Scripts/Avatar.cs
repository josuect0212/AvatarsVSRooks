using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Avatar
{
    public int spawnTime;
    public AvatarType avatarType;
    public int Spawner;
    public bool randomSpawn;
    public bool isSpawned;
}

public enum AvatarType
{
    Archer,
    ShieldBearer,
    Lumberjack,
    Cannibal
}

