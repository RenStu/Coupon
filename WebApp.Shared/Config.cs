using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public static class ApiKey
    {
        public static string UserName { get; set; }

        public static string Password { get; set; }
    }

    public class ApiKeyValue
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public static class Couch
    {
        public static string Protocol { get; set; }

        public static string Address { get; set; }

        public static string Port { get; set; }

        public static string EndPoint { get { return $"{Protocol}://{ApiKey.UserName}:{ApiKey.Password}@{Address}:{Port}"; } }

        public static string DBUsers { get { return "_users"; } }

        public static string DBOffers { get { return "Offers"; } }
    }

    public class CouchValue
    {
        public string Protocol { get; set; }

        public string Address { get; set; }

        public string Port { get; set; }
    }
}
