using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

public static class FirebaseNotificationService
{
    private static bool _initialized = false;

    public static void Initialize(IConfiguration configuration)
    {
        if (_initialized) return;

        var serviceAccountJson = configuration["FireBase:ServiceAccountJson"]
            ?? throw new InvalidOperationException("Missing ServiceAccountJson in configuration");

        var credential = GoogleCredential.FromJson(serviceAccountJson);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }

        _initialized = true;
    }

    public static async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };
        // Gửi notify
        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"✅ Successfully sent message: {response}");
    }
}
