using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarSpawner : MonoBehaviour
{
    public List<GameObject> avatarPrefabs;
    public List<Avatar> avatars;

    private void Update()
    {
        foreach (Avatar avatar in avatars)
        {
            if (avatar.isSpawned == false && avatar.spawnTime <= Time.time)
            {
                if (avatar.randomSpawn)
                {
                    avatar.Spawner = Random.Range(0, transform.childCount);
                }
                GameObject avatarInstance = Instantiate(avatarPrefabs[(int)avatar.avatarType], transform.GetChild(avatar.Spawner).transform);
                transform.GetChild(avatar.Spawner).GetComponent<SpawnPoint>().avatars.Add(avatarInstance);
                avatar.isSpawned = true;
            }
        }
    }
}
