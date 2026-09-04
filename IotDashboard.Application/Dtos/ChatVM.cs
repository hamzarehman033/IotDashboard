namespace IotDashboard.Application.Dtos
{
    public class ChatRequestVM
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatHistoryItemVM> History { get; set; } = new();

        /// <summary>Optional selected device id when the UI question needs a device.</summary>
        public long? DeviceId { get; set; }

        /// <summary>Optional selected device name/code when the UI question needs a device.</summary>
        public string? DeviceName { get; set; }
    }

    public class ChatHistoryItemVM
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatReplyVM
    {
        public string Answer { get; set; } = string.Empty;
    }
}
