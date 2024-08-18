using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniDOS.FileSystem
{
    class FileSystem
    {
        private CosmosVFS _vfs = new CosmosVFS();

        public FileSystem()
        {
            VFSManager.RegisterVFS(_vfs);
        }

        public bool MkDir(string path)
        {
            bool ret = VFSManager.DirectoryExists(path);
            if (!ret)
            {
                VFSManager.CreateDirectory(path);
                return true;
            }
            return false;
        }
    }
}
