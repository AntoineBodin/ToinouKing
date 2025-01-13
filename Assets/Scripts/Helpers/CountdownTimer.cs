using Assets.Scripts.Helpers;

/// <summary>
/// Timer that counts down from a specific value to zero.
/// </summary>
public class CountdownTimer : Timer
{
    public CountdownTimer(float value) : base(value) { }

    public override void Tick(float deltatime)
    {
        if (IsRunning && CurrentTime > 0)
        {
            CurrentTime -= deltatime;
        }

        if (IsRunning && CurrentTime <= 0)
        {
            Stop();
        }
    }

    public override bool IsFinished => CurrentTime <= 0;
}