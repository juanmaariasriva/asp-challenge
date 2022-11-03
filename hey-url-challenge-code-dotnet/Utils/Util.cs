using System.Text;
using System;

namespace hey_url_challenge_code_dotnet.Utils
{
    public class Util
    {
        public static string getShortUrl(int length)
        {
            const string src = "ABCDEFGHIJKLMNOPQRESTUVWXYZ";
            var sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(src[random.Next(0, src.Length)]);
            }
            return sb.ToString();
        }
    }
}
