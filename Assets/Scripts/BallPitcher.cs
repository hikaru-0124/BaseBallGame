using System;
using UnityEngine;

public class BallPitcher : MonoBehaviour
{
    [SerializeField] private Baseball ballPrefab;
    [SerializeField] private Transform pitchOrigin;
    [SerializeField] private Transform strikeZoneCenter;
    [SerializeField] private float pitchSpeed = 21f;
    [SerializeField] private float spreadX = 0.35f;
    [SerializeField] private float spreadY = 0.25f;
    [SerializeField] private float nextPitchDelay = 1.2f;

    private bool waitingNextPitch;
    private float queuedBatterZ;

    public Baseball CurrentBall { get; private set; }
    public float NextPitchDelay => nextPitchDelay;

    public event Action<Baseball> BallSpawned;

    public void Pitch(float batterPositionZ)
    {
        if (waitingNextPitch || ballPrefab == null || pitchOrigin == null || strikeZoneCenter == null)
        {
            return;
        }

        Vector3 spawnPos = pitchOrigin.position;
        CurrentBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        Vector3 target = strikeZoneCenter.position + new Vector3(
            UnityEngine.Random.Range(-spreadX, spreadX),
            UnityEngine.Random.Range(-spreadY, spreadY),
            0f
        );
        Vector3 direction = (target - spawnPos).normalized;
        CurrentBall.Launch(direction * pitchSpeed, batterPositionZ);
        BallSpawned?.Invoke(CurrentBall);
    }

    public void QueueNextPitch(float batterPositionZ)
    {
        if (waitingNextPitch)
        {
            return;
        }

        waitingNextPitch = true;
        queuedBatterZ = batterPositionZ;
        Invoke(nameof(DoNextPitch), nextPitchDelay);
    }

    private void DoNextPitch()
    {
        waitingNextPitch = false;
        Pitch(queuedBatterZ);
    }
}
