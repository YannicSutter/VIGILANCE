using System.Diagnostics;

public class GameLoop
{
    private readonly int tickRate;
    private readonly double tickIntervalMs;
    private readonly Stopwatch stopwatch = new Stopwatch();
    private double lastTickTime;

    public GameLoop(int tickRate)
    {
        this.tickRate = tickRate;
        this.tickIntervalMs = 1000.0 / tickRate;
    }

    public void Run(Action onPoll, Action<float> onTick)
    {
        stopwatch.Start();
        while (true)
        {
            onPoll();

            double now = stopwatch.Elapsed.TotalMilliseconds;
            if (now - lastTickTime >= tickIntervalMs)
            {
                float deltaTime = (float)((now - lastTickTime) / 1000.0);
                lastTickTime = now;
                onTick(deltaTime);
            }

            Thread.Sleep(1);
        }
    }
}