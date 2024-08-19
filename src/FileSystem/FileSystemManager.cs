using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;
using System;
using System.IO;

namespace MiniDOS.FileSystem
{
    public class FileSystemManager
    {
        private static string __CURRENT_DRIVE = "0:";
        private CosmosVFS _vfs;
        private string _currentDir = @"\";

        public string CurrentDir { get { return _currentDir; } }


        private string FullPath(string path)
        {
            return @$"{__CURRENT_DRIVE}.\{PreparePath(path)}";
        }

        private string PreparePath(string path)
        {
            if(path.Trim().Equals(".."))
            {
                var pathList = _currentDir.Split('\\');
                string _path = @"\";

                for(int i = 0; i < (pathList.Length - 1); i++)
                {
                    _path = Path.Combine(_path, pathList[i]);
                }

                return _path;
            }

            return @$"{Path.Combine(_currentDir, path)}";
        }

        public FileSystemManager()
        {
            _vfs = new CosmosVFS();
            VFSManager.RegisterVFS(_vfs);
        }

        public bool MkDir(string path, out string error)
        {
            string _path = FullPath(path);
            bool ret = VFSManager.DirectoryExists(_path);

            error = default;

            if (!ret)
            {
                try
                {
                    VFSManager.CreateDirectory(_path);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }
            
            error = "Directory already exists";

            return false;
        }

        public bool RmDir(string path, out string error)
        {
            string _path = FullPath(path);
            bool ret = VFSManager.DirectoryExists(_path);

            error = default;

            if (ret)
            {
                try
                {
                    VFSManager.DeleteDirectory(_path, true);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            error = "Directory not exists";

            return false;
        }

        public bool ChDir(string path, out string error)
        {
            string _path = PreparePath(path);
            bool ret = VFSManager.DirectoryExists(FullPath(_path));

            error = default;

            if (ret)
            {
                _currentDir = _path;
                return true;
            }

            error = "Directory not exists";

            return false;
        }

        public bool GetDir(string path)
        {
            try
            {
                string _path = FullPath(path);
                var dirList = VFSManager.GetDirectoryListing(_path);
                ConsoleColor color = Console.ForegroundColor;

                foreach (var file in dirList)
                {
                    bool isDir = (file.mEntryType == DirectoryEntryTypeEnum.Directory);
                    string type = (isDir ? "<DIR>" : "     ");
                    string size = (!isDir ? file.mSize.ToString() : "").PadLeft(19, ' ');
                    string format = $"{type}   {size}    {file.mName}";

                    if (isDir)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    Console.WriteLine(format);

                    if (isDir)
                    {
                        Console.ForegroundColor = color;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CopyFile(string source, string destination, out string error)
        {
            string _source = FullPath(source);
            string _destination = FullPath(destination);
            bool ret = VFSManager.FileExists(_source);

            error = default;

            if (ret)
            {
                try
                {
                    File.Copy(_source, _destination, false);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            error = "File not exists";

            return false;
        }

        public bool RenameFile(string oldFileName, string newFileName, out string error)
        {
            string _oldFileName = FullPath(oldFileName);
            string _newFileName = FullPath(newFileName);
            bool ret = VFSManager.FileExists(_oldFileName);

            error = default;

            if (ret)
            {
                try
                {
                    File.Copy(_oldFileName, _newFileName);
                    File.Delete(_oldFileName);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            error = "File not exists";

            return false;
        }

        public bool DeleteFile(string file, out string error)
        {
            string _file = FullPath(file);
            bool ret = VFSManager.FileExists(_file);

            error = default;

            if (ret)
            {
                try
                {
                    File.Delete(_file);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            error = "File not exists";

            return false;
        }

        public bool ReadFile(string file, out string error)
        {
            string _file = FullPath(file);
            bool ret = VFSManager.FileExists(_file);

            error = default;

            if (ret)
            {
                try
                {
                    var str = System.Text.Encoding.Default.GetString(File.ReadAllBytes(_file));

                    Console.WriteLine(str);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }

            error = "File not exists";

            return false;
        }
    }
}
