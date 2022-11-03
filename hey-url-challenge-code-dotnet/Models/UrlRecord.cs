using System;
using System.ComponentModel.DataAnnotations;

namespace hey_url_challenge_code_dotnet.Models
{
    public class UrlRecord
    {
        [Key]
        public Guid Id { get; set; }
        public String url { get; set; }
        public String browser { get; set; }
        public String platform { get; set; }
        public DateTime Created { get; set; }
    }
}
