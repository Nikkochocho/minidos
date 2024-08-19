using System;
using System.IO;

namespace MiniDOS.Shell
{
    public class Command
    {
        private readonly FileSystem.FileSystem _fs;
        private bool _shutdown = false;

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

        public Command( FileSystem.FileSystem fs )
        {
            _fs = fs;
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
                                string _path = @$"{_fs.CurrentDir}\{path}";

                                if (!_fs.MkDir(_path, out string error)) 
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
                                string _path = @$"{_fs.CurrentDir}\{path}";

                                if (!_fs.RmDir(_path, out string error))
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
                                string _file = @$"{_fs.CurrentDir}\{file}";

                                if (!_fs.DeleteFile(_file, out string error))
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
                                string _file = @$"{_fs.CurrentDir}\{file}";

                                if (!_fs.ReadFile(_file, out string error))
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
                                string _path = @$"{_fs.CurrentDir}\{path}";

                                return _fs.GetDir(_path);
                            }
                            return false;
                        }

                    case "copy":
                        {
                            if (GetTwoParms(parms, out string source, out string destination))
                            {
                                string _source = @$"{_fs.CurrentDir}\{source}";
                                string _destination = @$"{_fs.CurrentDir}\{destination}";

                                if (!_fs.CopyFile(_source, _destination, out string error))
                                {
                                    Console.WriteLine(error);
                                }
                                return true;
                            }
                            return false;
                        }

                    case "ren":
                        {
                            if (GetTwoParms(parms, out string ret1, out string ret2))
                            {
                                Console.WriteLine($"PARM REN {ret1} {ret2}");
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

                    default:
                        return parms[0].Length == 0;
                }
            }
            return false;
        }
    }
}
