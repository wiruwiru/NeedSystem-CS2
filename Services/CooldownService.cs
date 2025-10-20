namespace NeedSystem.Services;

public class CooldownService
{
    private DateTime _lastCommandTime = DateTime.MinValue;
    private readonly int _cooldownSeconds;

    public CooldownService(int cooldownSeconds)
    {
        _cooldownSeconds = cooldownSeconds;
    }

    public bool CanExecute(out int secondsRemaining)
    {
        var secondsSinceLastCommand = (int)(DateTime.Now - _lastCommandTime).TotalSeconds;
        secondsRemaining = _cooldownSeconds - secondsSinceLastCommand;
        return secondsRemaining <= 0;
    }

    public void UpdateLastExecution()
    {
        _lastCommandTime = DateTime.Now;
    }

    public void Reset()
    {
        _lastCommandTime = DateTime.MinValue;
    }
}