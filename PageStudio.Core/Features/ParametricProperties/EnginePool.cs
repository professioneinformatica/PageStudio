using Jint;

namespace PageStudio.Core.Features.ParametricProperties;

public class EnginePool(int maxPoolSize = 10)
{
    private readonly Lock _lock = new();
    private readonly Stack<Engine> _pool = new();

    public Engine Get()
    {
        lock (_lock)
        {
            if (_pool.Count > 0)
            {
                var engine = _pool.Pop();
                // Reset engine state if necessary
                // In Jint 3/4, a full reset might be complex depending on what was injected.
                // We assume we use it in a stateless way or re-setup context.
                return engine;
            }
        }

        return CreateNewEngine();
    }

    private static Engine CreateNewEngine()
    {
        return new Engine(options =>
        {
            options.LimitRecursion(50);
            options.TimeoutInterval(TimeSpan.FromMilliseconds(500));
            // In Jint 4.x MaxMemoryUsage is moved or handled differently, 
            // for now we stick to timeout and recursion limit.
        });
    }

    public void Return(Engine engine)
    {
        lock (_lock)
        {
            if (_pool.Count < maxPoolSize)
            {
                // Jint doesn't have a simple Reset() for the whole engine that clears all variables 
                // easily without recreating the context in some versions, 
                // but we can try to minimize leakage.
                _pool.Push(engine);
            }
        }
    }
}
