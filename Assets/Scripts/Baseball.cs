using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Baseball : MonoBehaviour
{
    [SerializeField] private float stopSpeedThreshold = 0.5f;
    [SerializeField] private float stopDuration = 0.5f;
    [SerializeField] private float cleanupSeconds = 15f;

    private Rigidbody rb;
    private float lowSpeedTimer;
    private bool notifiedStopped;
    private bool notifiedPassedBatter;
    private float batterZ;

    public bool IsHit { get; private set; }

    public event Action<Baseball> Stopped;
    public event Action<Baseball> PassedBatter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Destroy(gameObject, cleanupSeconds);
    }

    public void Launch(Vector3 velocity, float batterPositionZ)
    {
        batterZ = batterPositionZ;
        IsHit = false;
        notifiedStopped = false;
        notifiedPassedBatter = false;
        lowSpeedTimer = 0f;
        rb.velocity = velocity;
    }

    public void MarkHit(Vector3 velocity)
    {
        IsHit = true;
        rb.velocity = velocity;
    }

    private void Update()
    {
        if (!notifiedPassedBatter && !IsHit && transform.position.z <= batterZ - 0.4f)
        {
            notifiedPassedBatter = true;
            PassedBatter?.Invoke(this);
        }

        if (notifiedStopped || !IsHit)
        {
            return;
        }

        bool nearGround = transform.position.y <= 0.7f;
        if (rb.velocity.magnitude <= stopSpeedThreshold && nearGround)
        {
            lowSpeedTimer += Time.deltaTime;
            if (lowSpeedTimer >= stopDuration)
            {
                notifiedStopped = true;
                Stopped?.Invoke(this);
            }
        }
        else
        {
            lowSpeedTimer = 0f;
        }
    }
}
