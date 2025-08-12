using GorillaNotifications.Notifications;

namespace GorillaNotifications
{
    public static class NotificationHeader
    {
        public static void SendNotification(string notification, float duration = 5f)
        {
            OnScreenNotifications.Instance.SendNotification(notification, duration);
        }
    }
}