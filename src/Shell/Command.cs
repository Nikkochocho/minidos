﻿using MiniDOS.FileSystem;
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

                    case "dir":
                        {
                            if ((parms.Length >= 1) && (parms.Length <= 2))
                            {
                                string path = (parms.Length == 2 ? parms[1] : "");

                                return _fs.GetDir(path);
                            }
                            return false;
                        }

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

                    case "shutdown":
                        _shutdown = true;
                        break;

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
