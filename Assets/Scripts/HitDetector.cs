using System;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    [SerializeField] private BatterController batter;
    [SerializeField] private float minHitSpeed = 12f;
    [SerializeField] private float maxHitSpeed = 28f;
    [SerializeField] private float launchUpward = 0.35f;
    [SerializeField] private float sideRandomness = 0.18f;

    public event Action<Baseball> BallHit;

    private void OnTriggerEnter(Collider other)
    {
        if (batter == null || !batter.IsInHitWindow)
        {
            return;
        }

        if (!other.TryGetComponent(out Baseball ball) || ball.IsHit)
        {
            return;
        }

        float speed = Mathf.Lerp(minHitSpeed, maxHitSpeed, batter.SwingPower01);
        Vector3 forward = transform.root.forward;
        Vector3 side = transform.root.right * UnityEngine.Random.Range(-sideRandomness, sideRandomness);
        Vector3 launchDir = (forward + side + Vector3.up * launchUpward).normalized;
        ball.MarkHit(launchDir * speed);
        BallHit?.Invoke(ball);
    }
}
