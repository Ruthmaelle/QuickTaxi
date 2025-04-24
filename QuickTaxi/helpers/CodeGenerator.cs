using System;
using System.Text;

namespace QuickTaxi.Helpers
{
    public static class CodeGenerator
    {
        public static string GenerateVerificationCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                code.Append(chars[random.Next(chars.Length)]);
            }

            return code.ToString();
        }
    }
}
