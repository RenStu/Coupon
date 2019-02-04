using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class BaseDocument
    {
        private string _Type;

        public string _id { get; set; }

        public string DbName { get; set; }

        public string Type
        {
            get { return this.GetType().Name; }
            set { _Type = value; }
        }

        public string CqrsType { get; set; }

    }

    public static class Cqrs
    {
        public static string Command { get { return "command"; } }

        public static string Query { get { return "query"; } }
    }
}
