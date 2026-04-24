using System;

namespace BussinessObjects.DTOs.Admin
{
    public class ApiSettingDto
    {
        public Guid Id { get; set; }
        public string? GeminiApiKey { get; set; }
        public string? ColabApiUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
