using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;
using System;
using System.IO;

namespace MiniDOS.FileSystem
{
    public class FileSystemManager
    {
        private const string __CURRENT_DRIVE = "0:";
        private CosmosVFS _vfs;
        private string _currentDir = @"\";

        public string CurrentDir { get { return _currentDir; } }
        public string CurrentDrive { get { return __CURRENT_DRIVE; } }
        public CosmosVFS CurrentFileSystem { get { return _vfs; } }


        private string PreparePath(string path)
        {
            if(path.Trim().Equals(".."))
            {
                var pathList = _currentDir.Split('\\');
                string _path = @"\";

                for(int i = 0; i < (pathList.Length - 1); i++)
                {
                    var pathItem = pathList[i];

                    if (!pathItem.Equals(""))
                    {
                        _path = Path.Combine(_path, pathItem);
                    }
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

        public string GetAbsolutePath(string path)
        {
            return @$"{CurrentDrive}.\{PreparePath(path)}";
        }

        public bool MkDir(string path, out string error)
        {
            string _path = GetAbsolutePath(path);
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
            string _path = GetAbsolutePath(path);
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
            bool ret = VFSManager.DirectoryExists(GetAbsolutePath(_path));

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
                string _path = GetAbsolutePath(path);
                var dirList = VFSManager.GetDirectoryListing(_path);
                ConsoleColor color = Console.ForegroundColor;
                int dirCount = 0;
                int fileCount = 0;
                long fileSize = 0;

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
                        dirCount++;
                    }
                    else
                    {
                        fileSize += file.mSize;
                        fileCount++;
                    }
                }

                var fmt = string.Format("{0:#,0}", fileSize);

                Console.WriteLine($"        {fileCount} File(s) {fmt} bytes");
                fmt = string.Format("{0:#,0}", _vfs.GetTotalFreeSpace(CurrentDrive));
                Console.WriteLine($"        {dirCount} Dir(s) {fmt} bytes free");
                
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
            string _source = GetAbsolutePath(source);
            string _destination = GetAbsolutePath(destination);
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
            string _oldFileName = GetAbsolutePath(oldFileName);
            string _newFileName = GetAbsolutePath(newFileName);
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
            string _file = GetAbsolutePath(file);
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
            string _file = GetAbsolutePath(file);
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
