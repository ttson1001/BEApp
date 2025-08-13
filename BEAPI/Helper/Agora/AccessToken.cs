using System.Security.Cryptography;
using System.Text;

namespace BEAPI.Helper.Agora
{
    // Minimal legacy AccessToken (version 006) implementation for RTC tokens with userAccount
    // Supports privilege: JoinChannel and Publish (audio/video/screen) sharing same expiry
    internal static class RtcTokenBuilder
    {
        public enum Role
        {
            Publisher = 1,
            Subscriber = 2
        }

        public static string BuildTokenWithUserAccount(string appId, string appCertificate, string channelName, string userAccount, Role role, uint privilegeExpiredTs)
        {
            // token version 006
            var version = "006";

            // message: salt(4bytes) + ts(4bytes) + service(1) + service payload
            // We'll implement a simplified packer where privileges are for RTC join (1) and publish (2,3,4)

            // salt and ts
            var rnd = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            var salt = BitConverter.GetBytes(rnd);
            if (BitConverter.IsLittleEndian) Array.Reverse(salt);
            var ts = BitConverter.GetBytes(privilegeExpiredTs);
            if (BitConverter.IsLittleEndian) Array.Reverse(ts);

            // Build privileges buffer: key count + (privilege id + expire ts) pairs
            // For simplicity: include only join privilege (1) and publish audio(2), video(3), screen(4)
            var privileges = new List<byte>();
            void AppendPrivilege(ushort id, uint expire)
            {
                var idBytes = BitConverter.GetBytes(id);
                var expBytes = BitConverter.GetBytes(expire);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(idBytes);
                    Array.Reverse(expBytes);
                }
                privileges.AddRange(idBytes);
                privileges.AddRange(expBytes);
            }

            var roleIsPublisher = role == Role.Publisher;
            // privilege count (we always include at least join)
            var privilegeCount = (ushort)(roleIsPublisher ? 4 : 1);
            var countBytes = BitConverter.GetBytes(privilegeCount);
            if (BitConverter.IsLittleEndian) Array.Reverse(countBytes);

            var payload = new List<byte>();
            payload.AddRange(countBytes);
            AppendPrivilege(1, privilegeExpiredTs); // kJoinChannel = 1
            if (roleIsPublisher)
            {
                AppendPrivilege(2, privilegeExpiredTs); // kPublishAudioStream = 2
                AppendPrivilege(3, privilegeExpiredTs); // kPublishVideoStream = 3
                AppendPrivilege(4, privilegeExpiredTs); // kPublishDataStream = 4 (using as screen/data)
            }

            // Service type RTC: 1
            var serviceType = new byte[] { 0x01 };

            // pack service: serviceType + appId length + appId + channel length + channel + userAccount length + userAccount + salt + ts + privileges
            var buf = new List<byte>();
            buf.AddRange(serviceType);
            AppendString(buf, appId);
            AppendString(buf, channelName);
            AppendString(buf, userAccount);
            buf.AddRange(salt);
            buf.AddRange(ts);
            buf.AddRange(payload);

            var signing = HmacSha256(appCertificate, Concat(appId, channelName, userAccount, salt, ts, payload.ToArray()));

            var signature = ToBase64(signing);
            var content = ToBase64(buf.ToArray());
            return version + appId + signature + content;
        }

        private static byte[] Concat(string appId, string channel, string user, byte[] salt, byte[] ts, byte[] payload)
        {
            var b = new List<byte>();
            b.AddRange(Encoding.UTF8.GetBytes(appId));
            b.AddRange(Encoding.UTF8.GetBytes(channel));
            b.AddRange(Encoding.UTF8.GetBytes(user));
            b.AddRange(salt);
            b.AddRange(ts);
            b.AddRange(payload);
            return b.ToArray();
        }

        private static void AppendString(List<byte> buf, string s)
        {
            var data = Encoding.UTF8.GetBytes(s);
            var len = BitConverter.GetBytes((ushort)data.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(len);
            buf.AddRange(len);
            buf.AddRange(data);
        }

        private static byte[] HmacSha256(string key, byte[] message)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return h.ComputeHash(message);
        }

        private static string ToBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }
    }
}


