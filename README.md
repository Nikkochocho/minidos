# minidos

## :pushpin: Table of contents
- [About](#bulb-about)
- [Prerequisites](#dart-prerequisites)
- [Features](#tada-features)
- [HowTo](#rocket-howto)
- [Commands](#gear-commands)

## :bulb: About
`minidos` is a small DOS implementation, whose functions are quite similar to those of the original MS-DOS, with the difference of covering not only DOS commands but also Linux commands. The project was made using the C# language on Cosmos (C# Open Source Managed Operating System).

## :tada: Features
Here are the features `minidos` has to offer:
- A File System to manage files and directories
- Network Managment to receive data from the local host
- Memory Managment on server

## :dart: Prerequisites
In order to use the `minidos` application, it is necessary to obtain the following resources (install in the recommended order):
- The latest version of [Visual Studio](https://visualstudio.microsoft.com/pt-br/vs/community/).
- A [.NET Core 6.0](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0). A higher version is also supported.
- A virtual machine to get the operating system running. VMware Player is recommended. You can obtain the setup installation [here](https://www.vmware.com/products/desktop-hypervisor/workstation-and-fusion).
- [Cosmos OS SDK](https://github.com/Nikkochocho/Third_Party_Deps).
- `minidos` also has a dependency on the project [NLua](https://github.com/NLua/NLua) to get program running on Lua executables. You may acquire the NLua package through NuGet on [here](https://www.nuget.org/packages/NLua).
- A FTP Client software to run files on the virtual machine. [FileZilla Client](https://filezilla-project.org/download.php?type=client) is recommended.

## :rocket: HowTo
To get the `minidos` software running, you must first acquire all the [Prerequisites](#prerequisites) listed above.

## :gear: Commands
- `mkdir/md`
Creates a new directory.
```
md My_Dir
```
- `pwd/chdir/cd`
Changes to a different directory.
```
cd My_Dir
```
To return to **previous** directory:
```
cd ..
```
To return to **root** directory:
```
cd /
```
- `rmdir/rd`
Removes directory.
```
rd My_Dir
```
- `rm/del`
Removes file.
```
del My_File
```
- `cat/type`
Shows file properties.
```
type My_File
```
- `ls/dir`
Shows content of current directory.
```
dir
```
- `cp/copy`
Copies file.
```
copy My_File New_File
```
- `mv/ren`
Renames a file.
```
ren My_File My_File_Renamed
```
- `ipconfig`
Shows network information. 
```
ipconfig
```
In order to renew your IP adress, when first using this command you must use:
```
ipconfig /renew
```
Otherwise, the IP adress will be 0.0.0.0.
- `exec`
Runs a file.
```
exec 127.0.0.1:1999 lua_sample.lua
```
When using the ChatGPT API, you must add a forth parameter:
```
exec 127.0.0.1:1999 open_ai.lua "Your question here"
```
- `ftpserver`
Receives data sent from a FTP Client software.
```
ftpserver Folder_Name
```