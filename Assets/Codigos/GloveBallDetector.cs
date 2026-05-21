using UnityEngine;

public class GloveBallDetector : MonoBehaviour
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
            if (ballSpawner != null && ballSpawner.RegisterSave())
            {
                other.gameObject.SetActive(false);
                Destroy(other.gameObject);
            }
        }
    }
}
