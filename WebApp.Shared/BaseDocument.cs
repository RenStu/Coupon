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

        public string type
        {
            get { return string.IsNullOrEmpty(_Type) ? this.GetType().Name : _Type; }
            set { _Type = value; }
        }

        public string CqrsType { get; set; }

    }

    public static class Cqrs
    {
        public static string Command { get { return "commnad"; } }

        public static string Query { get { return "query"; } }
    }
}
