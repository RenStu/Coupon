using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    public static class Extensions
    {
        public static string HexToString(this string input)
        {
            byte[] raw = new byte[input.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return Encoding.ASCII.GetString(raw);
        }

        public static string StringToHex(this string input)
        {
            return String.Join("", input.Select(c => ((int)c).ToString("X2")));
        }

        public static string ToUserName(this string input)
        {
            return input.Substring(7).HexToString();
        }
    }
}
