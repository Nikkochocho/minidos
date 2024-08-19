using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using Cosmos.System.FileSystem.Listing;
using System.IO;

namespace MiniDOS.FileSystem
{
    public class FileSystem
    {
        private static string __CURRENT_DRIVE = "0:";
        private CosmosVFS _vfs;
        private string _currentDir = "";

        public string CurrentDir { get { return _currentDir; } }


        private string Normalize(string path)
        {
            return @$"{__CURRENT_DRIVE}.\{path}";
        }

        public FileSystem()
        {
            _vfs = new CosmosVFS();
            VFSManager.RegisterVFS(_vfs);
        }

        public bool MkDir(string path, out string error)
        {
            string _path = Normalize(path);
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
            string _path = Normalize(path);
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
            string _path = Normalize(path);
            bool ret = VFSManager.DirectoryExists(_path);

            error = default;

            if (ret)
            {
                _currentDir = path;
                return true;
            }

            error = "Directory not exists";

            return false;
        }

        public bool GetDir(string path)
        {
            try
            {
                string _path = Normalize(path);
                var dirList = VFSManager.GetDirectoryListing(_path);

                foreach (var file in dirList)
                {
                    string type = (file.mEntryType == DirectoryEntryTypeEnum.Directory ? "<DIR>" : "     ");
                    string size = (file.mEntryType == DirectoryEntryTypeEnum.File ? file.mSize.ToString() : "").PadLeft(19, ' ');
                    string format = $"{type}   {size}    {file.mName}";
                    
                    Console.WriteLine(format);
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
            string _source = Normalize(source);
            string _destination = Normalize(destination);
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

        public bool DeleteFile(string file, out string error)
        {
            string _file = Normalize(file);
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
            string _file = Normalize(file);
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
