using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float acceleration = 12f;
    public float deceleration = 18f;

    private Rigidbody2D rb;
    private float moveInput;
    private float currentVelocity;

    public List<Transform> chicks = new List<Transform>();
    public int targetChicks = 5;
    public int totalChicksInLevel = 0;

    public bool isGameOver = false;
    private bool stateLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isGameOver)
        {
            moveInput = 0;

            if (!stateLocked && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;

                stateLocked = true;
            }

            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        moveInput = Input.GetAxis("Horizontal");
#else
        moveInput = 0;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.position.x > Screen.width / 2)
                moveInput = 1f;
            else
                moveInput = -1f;
        }
#endif
    }

    void FixedUpdate()
    {
        if (isGameOver || rb == null) return;

        float targetVelocity = moveInput * moveSpeed;

        if (Mathf.Abs(moveInput) > 0.01f)
            currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        else
            currentVelocity = Mathf.Lerp(currentVelocity, 0, deceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(currentVelocity, rb.linearVelocity.y);

        // --- VISUALS ---
        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            // Rotate ONLY the visual
            float targetRotation = -currentVelocity * 3f;
            visual.rotation = Quaternion.Lerp(
                visual.rotation,
                Quaternion.Euler(0, 0, targetRotation),
                Time.fixedDeltaTime * 10f
            );

            // Flip ONLY the visual (or handle properly if it faces a specific way)
            if (currentVelocity > 0.05f)
                visual.localScale = new Vector3(-Mathf.Abs(visual.localScale.x), visual.localScale.y, visual.localScale.z);
            else if (currentVelocity < -0.05f)
                visual.localScale = new Vector3(Mathf.Abs(visual.localScale.x), visual.localScale.y, visual.localScale.z);
        }
    }

    void LateUpdate()
    {
        if (isGameOver) return;

        float limit = LevelGenerator.screenLimit;

        if (transform.position.x > limit)
            transform.position = new Vector3(-limit, transform.position.y, 0);
        else if (transform.position.x < -limit)
            transform.position = new Vector3(limit, transform.position.y, 0);
    }
}