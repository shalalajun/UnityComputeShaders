using UnityEngine;

public class SeekSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public Transform target;
    public int spawnCounter = 80;

    void Start()
    {
        for(int i=0; i<spawnCounter; i++)
        {
            SpawnBoid();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnBoid()
    {
        Vector3 randomPose = Random.insideUnitSphere * 10.0f;

        GameObject boids = Instantiate(boidPrefab, randomPose, Quaternion.identity); 

        Vehicle v = boids.GetComponent<Vehicle>();

        if(v != null)
        {
            v.target = this.target;
        }

        boids.transform.parent = this.transform;
    }
}
