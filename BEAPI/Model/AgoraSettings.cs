namespace BEAPI.Model
{
    public class AgoraSettings
    {
        public string AppId { get; set; } = string.Empty;
        public string AppCertificate { get; set; } = string.Empty; // optional
        public int TokenExpireSeconds { get; set; } = 3600;
    }
}


