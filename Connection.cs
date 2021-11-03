using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;

namespace MultiSSH
{
    public sealed class Connection : IDisposable
    {
        public string HostName { get; set; } = null;
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;
        public int? Port { get; set; } = null;

        private SshClient Client { get; }

        private Shell _shell;
        private MemoryStream _inputStream, _outputStream, _errorStream;

        private static UnicodeEncoding UnicodeEncoding { get; } = new();

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

        public void CreateShell()
        {
            _inputStream = new MemoryStream();
            _outputStream = new MemoryStream();
            _errorStream = new MemoryStream();

            _shell = Client.CreateShell(_inputStream, _outputStream, _errorStream);
            _shell.Start();
        }

        public void InitShell()
        {
            if (_shell is null) CreateShell();
        }

        public void Write(string command) => _inputStream.Write(UnicodeEncoding.GetBytes(command));

        public void Read(byte[] outputBuffer, byte[] errorBuffer)
        {
            _outputStream.Read(outputBuffer, 0, (int)_outputStream.Length);
            _errorStream.Read(errorBuffer, 0, (int)_errorStream.Length);
        }

        public (string, string) ReadText(byte[] outputBuffer, byte[] errorBuffer)
        {
            Read(outputBuffer, errorBuffer);

            return (UnicodeEncoding.GetString(outputBuffer), UnicodeEncoding.GetString(errorBuffer));
        }

        public void Flush()
        {
            _inputStream.Flush();
            _outputStream.Flush();
            _errorStream.Flush();
        }

        public void Connect() => Client.Connect();
        public void Disconnect() => Client.Disconnect();
        public void Dispose() => Client.Dispose();
        public void RunCommand(string commandText) => Client.RunCommand(commandText);
        public (int, string, string) RunCommandVerbose(string commandText)
        {
            var command = Client.CreateCommand(commandText);
            command.Execute();

            return (command.ExitStatus, command.Result, command.Error);
        }
    }
}