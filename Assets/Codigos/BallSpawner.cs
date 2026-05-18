using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private BoxCollider spawnZone;
    [SerializeField] private BoxCollider targetZone;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float launchSpeed = 10f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnBall), 0f, spawnInterval);
    }

    private void SpawnBall()
    {
        Vector3 spawnPosition = GetRandomPointInZone(spawnZone);
        Vector3 targetPosition = new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z - 10f);

        GameObject newBall = Instantiate(
            ballPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            Vector3 direction = (targetPosition - spawnPosition).normalized;
            rb.linearVelocity = direction * launchSpeed;
        }
    }

    private Vector3 GetRandomPointInZone(BoxCollider zone)
    {
        Bounds bounds = zone.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, randomY, randomZ);
    }
}