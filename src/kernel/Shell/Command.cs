using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using CosmosFtpServer;
using RPCLibrary.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace MiniDOS.Shell
{
    public class Command
    {
        private static string __YEAR = "2024";
        private static string __VERSION = "0.1";
        private static string __AUTHOR = "Lara H. Ferreira";

        private readonly FileSystem.FileSystemManager _fs;
        private bool _shutdown = false;

        public string CurrentDir { get { return _fs.CurrentDir; } }
        public bool Shutdown { get { return _shutdown; } }

        private IEnumerable<string> ParseCmdLineArgs(string line, char delimiter, char textQualifier)
        {
            if (line == null)
                yield break;
            else
            {
                char prevChar = '\0';
                char nextChar = '\0';
                char currentChar = '\0';
                bool inString = false;
                StringBuilder token = new StringBuilder();

                line = line.TrimEnd();

                for (int i = 0; i < line.Length; i++)
                {
                    currentChar = line[i];

                    if (i > 0)
                        prevChar = line[i - 1];
                    else
                        prevChar = '\0';

                    if (i + 1 < line.Length)
                        nextChar = line[i + 1];
                    else
                        nextChar = '\0';

                    if (currentChar == textQualifier && (prevChar == '\0' || prevChar == delimiter) && !inString)
                    {
                        inString = true;
                        continue;
                    }

                    if (currentChar == textQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
                    {
                        inString = false;
                        continue;
                    }

                    if (currentChar == delimiter && !inString)
                    {
                        yield return token.ToString();
                        token = token.Remove(0, token.Length);
                        continue;
                    }

                    token = token.Append(currentChar);
                }

                yield return token.ToString();
            }
        }

        private bool GetOneParm(string[] parms, out string ret)
        {
            ret = default;

            if (parms.Length == 2 && parms[1] != "")
            {
                ret = parms[1];

                return true;
            }
            return false;
        }

        private bool GetTwoParms(string[] parms, out string ret1, out string ret2)
        {
            ret1 = ret2 = default;

            if (parms.Length == 3 && parms[1] != "" && parms[2] != "")
            {
                ret1 = parms[1];
                ret2 = parms[2];

                return true;
            }
            return false;
        }
        private bool GetTwoParmsAndOptional(string[] parms, out string ret1, out string ret2, out string optional)
        {
            ret1 = ret2 = optional = default;

            if (parms.Length >= 3 && parms[1] != "" && parms[2] != "")
            {
                ret1 = parms[1];
                ret2 = parms[2];

                if (parms.Length == 4)
                {
                    optional = parms[3];
                }

                return true;
            }
            return false;
        }

        private string[]? GetHostPort(string hostPort)
        {
            var aHostPort = hostPort.Split(':');

            if (aHostPort.Length == 2)
                return aHostPort;

            return null;
        }

        public Command( FileSystem.FileSystemManager fs )
        {
            _fs = fs;
        }

        public void ShowTitle()
        {
            string license = $"minidos v{__VERSION}. CopyLeft (c) {__YEAR} by {__AUTHOR}\n" +
                 "This program comes with ABSOLUTELY NO WARRANTY;\n" +
                 "This is free software, and you are welcome to redistribute it under\n" +
                 "certain conditions.\n\n";

            Console.WriteLine(license);
        }

        public bool Exec(string cmd)
        {
            var parms = ParseCmdLineArgs(cmd, ' ', '\"').ToArray();

            if (parms.Length > 0)
            {
                string command = parms[0].Trim().ToLower();

                switch (command)
                {
                    case "pwd":
                    case "chdir":
                    case "cd":
                        {
                            if (!command.Equals("pwd") && GetOneParm(parms, out string path))
                            {
                                if (!_fs.ChDir(path, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                            }
                            else
                            {
                                Console.WriteLine(_fs.CurrentDir);
                            }

                            return true;
                        }

                    case "mkdir":
                    case "md":
                        {
                            if (GetOneParm(parms, out string path))
                            {
                                if (!_fs.MkDir(path, out string error)) 
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "rmdir":
                    case "rd":
                        {
                            if (GetOneParm(parms, out string path))
                            {
                                if (!_fs.RmDir(path, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "rm":
                    case "del":
                        {
                            if (GetOneParm(parms, out string file))
                            {
                                if (!_fs.DeleteFile(file, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "cat":
                    case "type":
                        {
                            if (GetOneParm(parms, out string file))
                            {
                                if (!_fs.ReadFile(file, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "ls":
                    case "dir":
                        {
                            if ((parms.Length >= 1) && (parms.Length <= 2))
                            {
                                string path = (parms.Length == 2 ? parms[1] : CurrentDir);

                                return _fs.GetDir(path);
                            }
                            return false;
                        }

                    case "cp":
                    case "copy":
                        {
                            if (GetTwoParms(parms, out string source, out string destination))
                            {
                                if (!_fs.CopyFile(source, destination, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "mv":
                    case "ren":
                        {
                            if (GetTwoParms(parms, out string oldFileName, out string newFileName))
                            {
                                if (!_fs.RenameFile(oldFileName, newFileName, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "ipconfig":
                        {
                            if (GetOneParm(parms, out string parm))
                            {
                                if (parm == "/renew".ToLower())
                                {
                                    using (var xClient = new DHCPClient())
                                    {
                                        xClient.SendDiscoverPacket();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid parameter");
                                    return false;
                                }
                            }
                            else
                            {
                                if (NetworkConfiguration.CurrentNetworkConfig != null)
                                {
                                    string cardType = NetworkConfiguration.CurrentNetworkConfig.Device.CardType == Cosmos.HAL.CardType.Wireless ? "Writeless" : "Ethernet";

                                    Console.WriteLine($"{cardType} adapter: ");
                                    Console.WriteLine();

                                    Console.WriteLine($"Description.......: {NetworkConfiguration.CurrentNetworkConfig.Device.Name} :");
                                    Console.WriteLine($"Device............: {NetworkConfiguration.CurrentNetworkConfig.Device.NameID}");
                                    Console.WriteLine($"IP Adress.........: {NetworkConfiguration.CurrentAddress.ToString()}");
                                }
                                else
                                {
                                    Console.WriteLine("IP Adress.........: 0.0.0.0");
                                }
                            }
                            return true;
                        }

                    case "exec":
                        {
                            if (GetTwoParmsAndOptional(parms, out string hostPort, out string filename, out string cmdLineParms))
                            {
                                string absFileNamePath = _fs.GetAbsolutePath(filename);
                                RPCExecution exec = new RPCExecution();
                                var aHostPort = GetHostPort(hostPort);

                                if (aHostPort == null)
                                {
                                    Console.WriteLine("Invalid hostname:port parameter. Is it in ip:port format ?");
                                    return false;
                                }

                                var hostname = aHostPort[0];
                                var port = int.Parse(aHostPort[1]);

                                if (exec.Execute(absFileNamePath, hostname, port, cmdLineParms))
                                {
                                    Console.WriteLine("Execution sucessfull");
                                }
                                else
                                {
                                    Console.WriteLine("Execution failed");
                                }

                                return true;
                            }
                            return false;
                        }

                    case "ftpserver":
                        {
                            if (GetOneParm(parms, out string path))
                            {
                                var absPath = _fs.GetAbsolutePath(path);

                                if (!absPath.EndsWith("\\"))
                                {
                                    absPath += "\\";
                                }

                                using (var ftpServer = new FtpServer(_fs.CurrentFileSystem, absPath, true))
                                {
                                    Console.WriteLine($"FTP files directory [{absPath}]");

                                    ftpServer.Listen();
                                }
                                return true;
                            }
                            return false;
                        }

                    case "shutdown":
                        _shutdown = true;
                        break;

                    case "clear":
                    case "cls":
                        Console.Clear();
                        return true;

                    case "ver":
                        ShowTitle();
                        return true;

                    default:
                        return parms[0].Length == 0;
                }
            }
            return false;
        }
    }
}
