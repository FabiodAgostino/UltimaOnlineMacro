namespace MQTT.Models
{
    public class MqttNotificationModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DeviceId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationSeverity Type { get; set; } = NotificationSeverity.Info;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public Color IconBackground { get; set; }
        public string IconText { get; set; }


        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Timestamp;

                if (timeSpan.TotalMinutes < 1)
                    return "adesso";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} min fa";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} ore fa";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} giorni fa";

                return Timestamp.ToString("dd/MM/yyyy");
            }
        }

        public enum NotificationSeverity
        {
            Info,
            Warning,
            Error
        }
    }
}
