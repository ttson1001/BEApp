using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;

public static class FirebaseNotificationService
{
    private static bool _initialized = false;

    public static void Initialize(IConfiguration configuration)
    {
        if (_initialized) return;

        var firebaseSection = configuration.GetSection("FireBase");
        var serviceAccountJson = JsonSerializer.Serialize(firebaseSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value));

        var credential = GoogleCredential.FromJson(serviceAccountJson);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = credential
            });
        }

        _initialized = true;
    }

    public static async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new Message()
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"✅ Successfully sent message: {response}");
    }
}
