using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceAI : MonoBehaviour
{
    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float rotateSpeed = 120f;  // degrees per second
    public float predictionTime = 0.4f; // how far ahead police predicts

    [Header("Siren Light")]
    public SpriteRenderer sirenLight;  // assign a small light sprite
    public Color redColor = Color.red;
    public Color blueColor = Color.blue;
    float sirenTimer;
    bool sirenRed = true;

    Rigidbody2D rb;
    Transform playerTransform;
    Rigidbody2D playerRb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find the player using singleton
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
            playerRb = Player.Instance.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        FlashSiren();
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Predict where the player will be
        Vector2 predictedPos = (Vector2)playerTransform.position
                              + playerRb.linearVelocity * predictionTime;

        // Direction from police to predicted player position
        Vector2 dir = predictedPos - (Vector2)transform.position;

        // Rotate police car smoothly toward player
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle, targetAngle, rotateSpeed * Time.fixedDeltaTime);
        transform.eulerAngles = new Vector3(0, 0, newAngle);

        // Move forward � speed matches player + small bonus
        float currentSpeed = chaseSpeed + (GameManager.Instance != null
            ? GameManager.Instance.difficultyBonus : 0f);
        rb.linearVelocity = transform.up * currentSpeed;
    }

    void FlashSiren()
    {
        if (sirenLight == null) return;
        sirenTimer += Time.deltaTime;
        if (sirenTimer >= 0.2f)  // flash every 0.2 seconds
        {
            sirenTimer = 0;
            sirenRed = !sirenRed;
            sirenLight.color = sirenRed ? redColor : blueColor;
        }
    }

    // Called by GameManager when spawning � set start position
    public void SetSpawnPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
