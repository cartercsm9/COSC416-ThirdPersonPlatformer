using UnityEngine;
using TMPro;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private int coinValue = 1;
    [SerializeField] private TextMeshProUGUI scoreText; // Assign your TextMeshPro score UI element in the inspector

    // Using a static score variable so all coins add to the same score.
    private static int score = 0;

    void Update()
    {
        // Rotate the coin around its Y-axis.
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered is tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Increase the score
            score += coinValue;
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score.ToString();
            }
            // Destroy the coin after collection
            Destroy(gameObject);
        }
    }
}
