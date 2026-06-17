using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    
    public class EmailDTO
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        //public List<string> To { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
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
