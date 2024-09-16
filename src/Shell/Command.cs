﻿using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using MiniDOS.FileSystem;
using System;


namespace MiniDOS.Shell
{
    public class Command
    {
        private static string __YEAR = "2024";
        private static string __VERSION = "0.1";
        private static string __AUTHOR = "Lara H. Ferreira";

        private readonly FileSystemManager _fs;
        private bool _shutdown = false;

        public string CurrentDir { get { return _fs.CurrentDir; } }
        public bool Shutdown { get { return _shutdown; } }

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
            var parms = cmd.Split(' ');

            if (parms.Length > 0)
            {
                switch (parms[0].Trim().ToLower())
                {
                    case "chdir":
                    case "cd":
                        {
                            if (GetOneParm(parms, out string path))
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
