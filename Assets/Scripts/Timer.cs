using UnityEngine;

public class Timer
{
    public float interval;
    private float startTime;

    public Timer(float seconds)
    {
        interval = seconds;
        startTime = Time.time;
    }

    public void Reset()
    {
        startTime = Time.time;
    }

    public bool Elapsed
    {
        get
        {
            return Time.time - startTime >= interval;
        }
    }
}
