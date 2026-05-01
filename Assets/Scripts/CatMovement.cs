using UnityEngine;

public class CatMovement : MonoBehaviour
{
    public float speed = 2f;

    private float baseY;
    private float direction = 1f;

    private float leftLimit;
    private float rightLimit;

    void Start()
    {
        baseY = transform.position.y;

        // ✅ FIX: give each cat a tiny different Z to stop flicker
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            Random.Range(-0.1f, 0.1f)
        );

        // Detect platform below (Filter out small objects like chicks using RaycastAll)
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 5f);
        Collider2D validFloor = null;

        foreach (var h in hits)
        {
            // Platforms are always wide (> 2 units). Chicks/Cats are small.
            if (h.collider != null && h.collider.bounds.size.x > 1.5f)
            {
                validFloor = h.collider;
                break;
            }
        }

        if (validFloor != null)
        {
            float platformWidth = validFloor.bounds.size.x;
            float platformCenter = validFloor.transform.position.x;

            leftLimit = platformCenter - (platformWidth / 2f) + 0.5f;
            rightLimit = platformCenter + (platformWidth / 2f) - 0.5f;
        }
        else
        {
            // fallback
            leftLimit = transform.position.x - 2f;
            rightLimit = transform.position.x + 2f;
        }
    }

    void Update()
    {
        float moveX = direction * speed * Time.deltaTime;

        float newX = transform.position.x + moveX;

        // Turn at edges
        if (newX > rightLimit)
        {
            direction = -1f;
        }
        else if (newX < leftLimit)
        {
            direction = 1f;
        }

        // Flip sprite (works with your Visual child)
        if (direction > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Smooth bobbing
        float bob = Mathf.Sin(Time.time * 5f) * 0.1f;

        // ✅ Single final position (no double movement = no jitter)
        transform.position = new Vector3(
            transform.position.x + moveX,
            baseY + bob,
            transform.position.z
        );
    }
}