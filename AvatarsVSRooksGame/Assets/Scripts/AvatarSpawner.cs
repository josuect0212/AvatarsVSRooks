using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarSpawner : MonoBehaviour
{
    public List<GameObject> avatarPrefabs;  // Prefabs for each avatar type
    public float spawnInterval = 5f;        // How often to spawn
    private float nextSpawnTime;
    public int maxAvatars = 10;
    private int spawnedCount = 0;
    public bool randomSpawnPoint = true;
    public List<GameObject> activeAvatars = new List<GameObject>();

    private void Update()
    {
        if (Time.time >= nextSpawnTime && spawnedCount < maxAvatars)
        {
            SpawnRandomAvatar();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnRandomAvatar()
    {
        if (avatarPrefabs.Count == 0 || transform.childCount == 0) return;
        // Random avatar type
        int randomType = Random.Range(0, avatarPrefabs.Count);

        // Random spawn point
        int randomSpawner = Random.Range(0, transform.childCount);
        Transform spawnerTransform = transform.GetChild(randomSpawner);

        // Instantiate avatar
        GameObject avatarInstance = Instantiate(avatarPrefabs[randomType], spawnerTransform);

        // Register it in the SpawnPoint’s avatar list
        //spawnerTransform.GetComponent<SpawnPoint>().avatars.Add(avatarInstance);
        SpawnPoint spawnPoint = spawnerTransform.GetComponent<SpawnPoint>();
        if (spawnPoint != null)
        {
            spawnPoint.avatars.Add(avatarInstance);
        }
        activeAvatars.Add(avatarInstance);
        spawnedCount++;
        Debug.Log("Avatars Spawneados:" +spawnedCount+ " Avatars vivos:" + activeAvatars.Count);
    }
    public void OnAvatarDestroyed(GameObject avatar)
    {
        activeAvatars.Remove(avatar);
        CheckIfAllDefeated();
    }
        public bool AllAvatarsDefeated()
    {
        // No more avatars and we reached spawn limit
        return spawnedCount >= maxAvatars && activeAvatars.Count == 0;
    }
    private void CheckIfAllDefeated()
    {
        if (AllAvatarsDefeated())
        {
            FindObjectOfType<GameManager>().CheckWinCondition();
        }
    }
}
