using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Myth.Spread.Handlers {
    public class RemoteHost {
        private static string CurrentUserName { get; } = Environment.UserName;
        private string HostString { get; }
        public string Host { get; }
        private readonly string _userName;
        public string UserName => _userName ?? CurrentUserName;

        public RemoteHost(string hostString) {
            HostString = hostString;
            Host = hostString;

            string[] nodes = hostString.Split('@', 2);

            if (nodes.Length == 2) {
                _userName = nodes[0];
                Host = nodes[1];
            }
            else {
                Host = hostString;
            }
        }
    }
}