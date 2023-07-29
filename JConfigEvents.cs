using System;

public class JConfigEvents
{
    public static event Action<JConfig> OnLoaded;
    public static event Action<JConfig> OnUpdated;
    public static event Action<JConfig, Exception> OnError;

    public static void Invoke_OnLoaded(JConfig jConfig)
    {
        OnLoaded?.Invoke(jConfig);
    }

    public static void Invoke_OnUpdated(JConfig jConfig)
    {
        OnUpdated?.Invoke(jConfig);
    }

    public static void Invoke_OnError(JConfig jConfig, Exception exception)
    {
        OnError?.Invoke(jConfig, exception);
    }
}