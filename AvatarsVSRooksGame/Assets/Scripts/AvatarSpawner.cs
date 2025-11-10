using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarSpawner : MonoBehaviour
{
    public List<GameObject> avatarPrefabs;  // Prefabs for each avatar type
    public float spawnInterval = 5f;        // How often to spawn
    private float nextSpawnTime;
    public bool randomSpawnPoint = true;

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomAvatar();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnRandomAvatar()
    {
        // Random avatar type
        int randomType = Random.Range(0, avatarPrefabs.Count);

        // Random spawn point
        int randomSpawner = Random.Range(0, transform.childCount);
        Transform spawnerTransform = transform.GetChild(randomSpawner);

        // Instantiate avatar
        GameObject avatarInstance = Instantiate(avatarPrefabs[randomType], spawnerTransform);

        // Register it in the SpawnPoint’s avatar list
        spawnerTransform.GetComponent<SpawnPoint>().avatars.Add(avatarInstance);
    }
}
