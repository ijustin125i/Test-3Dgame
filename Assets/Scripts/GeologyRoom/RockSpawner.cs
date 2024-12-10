using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    // The rock prefab to spawn
    public GameObject rockPrefab;
    
    // The position where the rocks will spawn (top of the hill)
    public Transform spawnPoint;
    
    // Time between each rock spawn
    public float spawnInterval = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Start the spawning process
        InvokeRepeating("SpawnRock", 0f, spawnInterval);
    }

    // Method to spawn a rock
    void SpawnRock()
    {
        // Spawn a new rock at the spawn point with no rotation
        Instantiate(rockPrefab, spawnPoint.position, Quaternion.identity);
    }
}
