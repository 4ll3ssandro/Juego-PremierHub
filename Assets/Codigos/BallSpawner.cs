using UnityEngine;
using System.Collections;
using TMPro;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private BoxCollider spawnZone;
    [SerializeField] private BoxCollider targetZone;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private int ballsPerRound = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float minSecondsBetweenSpawns = 2f;
    [SerializeField] private TMP_Text ballsRemainingText;

    private int ballsRemaining;

    private void Start()
    {
        ballsRemaining = ballsPerRound;
        UpdateBallsRemainingText();
        StartCoroutine(SpawnRound());
    }

    private IEnumerator SpawnRound()
    {
        float[] spawnTimes = GetRandomSpawnTimes();
        float elapsedTime = 0f;

        for (int i = 0; i < spawnTimes.Length; i++)
        {
            float waitTime = spawnTimes[i] - elapsedTime;

            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            SpawnBall();
            ballsRemaining--;
            UpdateBallsRemainingText();
            elapsedTime = spawnTimes[i];
        }
    }

    private void SpawnBall()
    {
        Vector3 spawnPosition = GetRandomPointInZone(spawnZone);
        Vector3 targetPosition = GetRandomPointInZone(targetZone);

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

    private float[] GetRandomSpawnTimes()
    {
        float[] randomOffsets = new float[ballsPerRound];
        float availableRandomTime = roundDuration - minSecondsBetweenSpawns * (ballsPerRound - 1);

        if (availableRandomTime < 0f)
        {
            availableRandomTime = 0f;
        }

        for (int i = 0; i < randomOffsets.Length; i++)
        {
            randomOffsets[i] = Random.Range(0f, availableRandomTime);
        }

        System.Array.Sort(randomOffsets);

        for (int i = 0; i < randomOffsets.Length; i++)
        {
            randomOffsets[i] += minSecondsBetweenSpawns * i;
        }

        return randomOffsets;
    }

    private void UpdateBallsRemainingText()
    {
        if (ballsRemainingText != null)
        {
            ballsRemainingText.text = "Balones restantes: " + ballsRemaining;
        }
    }
}
