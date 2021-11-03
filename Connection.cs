using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace MultiSSH
{
    public sealed class Connection : IDisposable
    {
        public string HostName { get; set; } = null;
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;
        public int? Port { get; set; } = null;

        private SshClient Client { get; set; }

        public Connection(string hostName, string username, string password = null, int? port = null)
        {
            HostName = hostName;
            Username = username;
            Password = password;
            Port = port;

            AuthenticationMethod auth = Password switch
            {
                null => new PasswordAuthenticationMethod(Username, Password),
                _ => new NoneAuthenticationMethod(Username),
            };

            ConnectionInfo connectionInfo = new(HostName, Port ?? 22, Username, auth);

            Client = new SshClient(connectionInfo);
        }

        public void Connect() => Client.Connect();
        public void Disconnect() => Client.Disconnect();
        public void Dispose() => Client.Dispose();
        public void RunCommand(string command) => Client.RunCommand(command);

    }
}
