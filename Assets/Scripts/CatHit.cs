using UnityEngine;
using System.Collections;

public class CatHit : MonoBehaviour
{
    public ParticleSystem hitEffect;

    private static float lastHitTime = 0f;
    private float hitCooldown = 1.5f;

    private Coroutine currentSquash;

    private GameFlowManager gameFlowManager;

    private void Awake()
    {
        gameFlowManager = FindFirstObjectByType<GameFlowManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.unscaledTime - lastHitTime < hitCooldown) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.isGameOver) return;

        lastHitTime = Time.unscaledTime;

        UIManager ui = FindFirstObjectByType<UIManager>();

        if (ui != null)
        {
            ui.TriggerDamageFlash();
        }

        if (player.chicks.Count > 0)
        {
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake();
            }

            if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(direction * 2f, rb.linearVelocity.y);
            }

            if (currentSquash != null)
            {
                StopCoroutine(currentSquash);
                player.transform.localScale = GetBaseScale(player.transform);
            }

            currentSquash = StartCoroutine(SquashEffect(player.transform));

            Transform lastChick = player.chicks[player.chicks.Count - 1];

            if (hitEffect != null)
            {
                Instantiate(hitEffect, lastChick.position, Quaternion.identity);
            }

            player.chicks.RemoveAt(player.chicks.Count - 1);
            Destroy(lastChick.gameObject);
        }
        else
        {
            if (ui != null)
            {
                ui.ShowFail();
            }

            if (gameFlowManager != null)
            {
                gameFlowManager.ShowLoseScreen();
            }
        }
    }

    private Vector3 GetBaseScale(Transform t)
    {
        float sign = Mathf.Sign(t.localScale.x);
        return new Vector3(1f * sign, 1f, 1f);
    }

    private IEnumerator SquashEffect(Transform target)
    {
        if (target == null) yield break;

        float sign = Mathf.Sign(target.localScale.x);

        Vector3 baseScale = new Vector3(1f, 1f, 1f);
        Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);

        float duration = 0.08f;
        float t = 0f;

        while (t < duration)
        {
            if (target == null) yield break;

            t += Time.unscaledDeltaTime;

            Vector3 s = Vector3.Lerp(baseScale, squashScale, t / duration);
            s.x *= sign;

            target.localScale = s;
            yield return null;
        }

        t = 0f;

        while (t < duration)
        {
            if (target == null) yield break;

            t += Time.unscaledDeltaTime;

            Vector3 s = Vector3.Lerp(squashScale, baseScale, t / duration);
            s.x *= sign;

            target.localScale = s;
            yield return null;
        }

        if (target != null)
        {
            target.localScale = new Vector3(baseScale.x * sign, baseScale.y, baseScale.z);
        }
    }
}