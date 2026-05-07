using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BallPitcher pitcher;
    [SerializeField] private BatterController batter;
    [SerializeField] private HitDetector hitDetector;
    [SerializeField] private float hitDistanceThreshold = 15f;
    [SerializeField] private Vector3 homeBasePosition = Vector3.zero;

    private string message = "Spaceでスイング";
    private int hits;
    private int outs;
    private int strikes;

    private void OnEnable()
    {
        if (pitcher != null)
        {
            pitcher.BallSpawned += OnBallSpawned;
        }

        if (hitDetector != null)
        {
            hitDetector.BallHit += OnBallHit;
        }
    }

    private void OnDisable()
    {
        if (pitcher != null)
        {
            pitcher.BallSpawned -= OnBallSpawned;
        }

        if (hitDetector != null)
        {
            hitDetector.BallHit -= OnBallHit;
        }
    }

    private void Start()
    {
        if (pitcher != null && batter != null)
        {
            pitcher.Pitch(batter.transform.position.z);
        }
    }

    private void OnBallSpawned(Baseball ball)
    {
        ball.PassedBatter += OnPassedBatter;
        ball.Stopped += OnBallStopped;
        message = "来球中...";
    }

    private void OnBallHit(Baseball ball)
    {
        message = "打球！";
    }

    private void OnPassedBatter(Baseball ball)
    {
        strikes++;
        message = "ストライク！";
        QueueNextPitch();
    }

    private void OnBallStopped(Baseball ball)
    {
        float distance = Vector3.Distance(
            new Vector3(homeBasePosition.x, 0f, homeBasePosition.z),
            new Vector3(ball.transform.position.x, 0f, ball.transform.position.z)
        );

        if (distance >= hitDistanceThreshold)
        {
            hits++;
            message = $"ヒット！ ({distance:F1}m)";
        }
        else
        {
            outs++;
            message = $"アウト ({distance:F1}m)";
        }

        QueueNextPitch();
    }

    private void QueueNextPitch()
    {
        if (pitcher == null || batter == null)
        {
            return;
        }

        pitcher.QueueNextPitch(batter.transform.position.z);
    }

    private void OnGUI()
    {
        const int width = 350;
        const int height = 110;
        GUI.Box(new Rect(10, 10, width, height), "BaseBall Prototype");
        GUI.Label(new Rect(20, 40, width - 20, 25), $"結果: {message}");
        GUI.Label(new Rect(20, 62, width - 20, 25), $"HIT: {hits}  OUT: {outs}  STRIKE: {strikes}");
        GUI.Label(new Rect(20, 84, width - 20, 25), "操作: Space = Swing");
    }
}
