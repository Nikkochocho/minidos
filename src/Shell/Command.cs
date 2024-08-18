using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniDOS.Shell
{
    public class Command
    {
        private FileSystem.FileSystem _fs = new FileSystem.FileSystem();
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


        public bool Exec(string cmd)
        {
            var parms = cmd.Split(' ');

            if (parms.Length > 0)
            {
                switch (parms[0].ToLower())
                {
                    case "dir":
                        {
                            if (parms.Length == 1)
                            {
                                return true;
                            }
                            return false;
                        }

                    case "cd":
                        {
                            if (GetOneParm(parms, out string ret))
                            {
                                Console.WriteLine("PARM CD " + ret);
                                return true;
                            }
                            return false;
                        }

                    case "md":
                        {
                            if (GetOneParm(parms, out string path))
                            {
                                if (!_fs.MkDir(path)) 
                                {
                                    Console.WriteLine("Error to create directory");
                                }
                                return true;
                            }
                            return false;
                        }

                    case "rd":
                        {
                            if (GetOneParm(parms, out string ret))
                            {
                                Console.WriteLine("PARM RD " + ret);
                                return true;
                            }
                            return false;
                        }

                    case "del":
                        {
                            if (GetOneParm(parms, out string ret))
                            {
                                Console.WriteLine("PARM DEL " + ret);
                                return true;
                            }
                            return false;
                        }

                    case "copy":
                        {
                            if (GetTwoParms(parms, out string ret1, out string ret2))
                            {
                                Console.WriteLine($"PARM COPY {ret1} {ret2}");
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

                    default:
                        return parms[0].Length == 0;
                }
            }
            return false;
        }
    }
}
