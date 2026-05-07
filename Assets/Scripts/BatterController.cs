using UnityEngine;

public class BatterController : MonoBehaviour
{
    [SerializeField] private Transform batPivot;
    [SerializeField] private float swingDuration = 0.24f;
    [SerializeField] private float swingAngle = 95f;
    [SerializeField] private float cooldown = 0.15f;
    [SerializeField] private float hitWindowStart = 0.3f;
    [SerializeField] private float hitWindowEnd = 0.75f;

    private bool swinging;
    private float swingTimer;
    private float cooldownTimer;
    private Quaternion restRotation;
    private Quaternion endRotation;

    public bool IsInHitWindow { get; private set; }
    public float SwingPower01 { get; private set; }

    private void Start()
    {
        if (batPivot == null)
        {
            return;
        }

        restRotation = batPivot.localRotation;
        endRotation = restRotation * Quaternion.Euler(-swingAngle, 0f, 0f);
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (!swinging && cooldownTimer <= 0f && Input.GetKeyDown(KeyCode.Space))
        {
            swinging = true;
            swingTimer = 0f;
            IsInHitWindow = false;
            SwingPower01 = 0f;
        }

        if (!swinging || batPivot == null)
        {
            return;
        }

        swingTimer += Time.deltaTime;
        float t = Mathf.Clamp01(swingTimer / swingDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);
        batPivot.localRotation = Quaternion.Slerp(restRotation, endRotation, eased);

        IsInHitWindow = t >= hitWindowStart && t <= hitWindowEnd;
        if (IsInHitWindow)
        {
            float center = (hitWindowStart + hitWindowEnd) * 0.5f;
            float width = Mathf.Max(0.001f, (hitWindowEnd - hitWindowStart) * 0.5f);
            SwingPower01 = 1f - Mathf.Clamp01(Mathf.Abs(t - center) / width);
        }
        else
        {
            SwingPower01 = 0f;
        }

        if (t >= 1f)
        {
            swinging = false;
            IsInHitWindow = false;
            SwingPower01 = 0f;
            batPivot.localRotation = restRotation;
            cooldownTimer = cooldown;
        }
    }
}
