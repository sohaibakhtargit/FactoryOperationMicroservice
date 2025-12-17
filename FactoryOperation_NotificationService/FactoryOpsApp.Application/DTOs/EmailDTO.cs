namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs
{
    
    public class EmailDTO
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool? IsHtml { get; set; } = true;
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public List<EmailAttachment> Attachments { get; set; } = new();
    }

    public class EmailAttachment
    {
        public string FileName { get; }
        public byte[] Content { get; }

        public EmailAttachment(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }
    }

}
