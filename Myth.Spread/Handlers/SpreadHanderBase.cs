using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using Myth.Spread.Arguments;
using Renci.SshNet;

namespace Myth.Spread.Handlers {
    public class SpreadHanderBase {
        protected static IList<RemoteHost> RemoteHostList { get; }

        private static string SshPrivateKeyPath { get; }

        static SpreadHanderBase() {
            IList<RemoteHost> remoteHostList = new List<RemoteHost>();
            if (File.Exists(GlobalConfigPath)) {
                using (StreamReader streamReader = new StreamReader(GlobalConfigPath)) {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) {
                        remoteHostList.Add(new RemoteHost(line));
                    }
                }
            }

            string hostName = Dns.GetHostName();
            remoteHostList = remoteHostList.Where(remoteHost => {
                if (string.Equals(remoteHost.Host, hostName, StringComparison.CurrentCultureIgnoreCase)) {
                    if (CommandLine.IsVerbose) {
                        Console.WriteLine($"跳过当前节点:{hostName}");
                    }

                    return false;
                }

                return true;
            }).ToImmutableList();

            if (CommandLine.IsVerbose) {
                Console.WriteLine("远程节点列表：");
                foreach (RemoteHost remoteHost in remoteHostList) {
                    Console.WriteLine($"  {remoteHost}");
                }
            }

            RemoteHostList = remoteHostList;
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathTemp = Path.Combine(userFolderPath, ".ssh", "id_rsa");
            SshPrivateKeyPath = File.Exists(pathTemp) ? pathTemp : null;
        }

        public static string GlobalConfigPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Myth", "Spread", "RemoteHosts.conf");

        public static ConnectionInfo CreateConnectionInfo(RemoteHost remoteHost) {
            IList<AuthenticationMethod> authenticationMethodList = new List<AuthenticationMethod>();
            if (File.Exists("id_rsa")) {
                authenticationMethodList.Add(
                    new PrivateKeyAuthenticationMethod(remoteHost.UserName, new PrivateKeyFile("id_rsa")));
            }

            if (SshPrivateKeyPath != null) {
                authenticationMethodList.Add(new PrivateKeyAuthenticationMethod(remoteHost.UserName,
                    new PrivateKeyFile(SshPrivateKeyPath)));
            }

            //authenticationMethodList.Add(new KeyboardInteractiveAuthenticationMethod(remoteHost.UserName));
            return new ConnectionInfo(remoteHost.Host, remoteHost.UserName, authenticationMethodList.ToArray());
        }
    }
}