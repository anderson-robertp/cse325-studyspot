public class NotificationService
{
    public string? SuccessMessage { get; private set; }

    public void ShowSuccess(string message)
    {
        SuccessMessage = message;
    }

    public void Clear()
    {
        SuccessMessage = null;
    }
}