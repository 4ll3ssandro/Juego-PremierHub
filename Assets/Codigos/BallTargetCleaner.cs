using UnityEngine;

public class BallTargetCleaner : MonoBehaviour
{
    [SerializeField] private BallSpawner ballSpawner;

    private void Start()
    {
        if (ballSpawner == null)
        {
            ballSpawner = FindFirstObjectByType<BallSpawner>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (ballSpawner != null && ballSpawner.RegisterMiss())
            {
                other.gameObject.SetActive(false);
                Destroy(other.gameObject);
            }
        }
    }
}
