# minidos

## :pushpin: Table of contents
- [About](#bulb-about)
- [Prerequisites](#dart-prerequisites)
- [HowTo](#rocket-howto)
- [Commands](#gear-commands)

## :bulb: About
`minidos` is a small DOS implementation, whose functions are quite similar to those of the original MS-DOS, with the difference of covering not only DOS commands but also Linux commands. The project was made using the C# language on Cosmos (C# Open Source Managed Operating System).

## :dart: Prerequisites
In order to use the minidos application, it is necessary to obtain the following resources (install in the recommended order):
- The latest version of [Visual Studio](https://visualstudio.microsoft.com/pt-br/vs/community/).
- A [.NET Core 6.0](https://dotnet.microsoft.com/pt-br/download/dotnet/6.0). A higher version is also supported.
- A virtual machine to get the operating system running. VMware Player is recommended. You can obtain the setup installation [here](https://www.vmware.com/products/desktop-hypervisor/workstation-and-fusion).
- [Cosmos OS SDK](https://github.com/Nikkochocho/Third_Party_Deps).
- `minidos` also has a dependency on the project [NLua](https://github.com/NLua/NLua) to get program running on Lua executables. You may acquire the NLua package through NuGet on [here](https://www.nuget.org/packages/NLua).
- A FTP Client software to run files on the virtual machine. [FileZilla Client](https://filezilla-project.org/download.php?type=client) is recommended.

## :rocket: HowTo
To get the `minidos` software runnning, you must first acquire all the [Prerequisites](#prerequisites) listed above.

## :gear: Commands
### :space_invader: Using Cosmos
- `mkdir/md`
Creates a new directory.

```
mkdir My_Dir
```

- `pwd/chdir/cd`
Changes directory.

- `rmdir/rd`
Removes directory.

- `rm/del`
Removes file.

- `cat/type`
Shows file properties.

- `ls/dir`
Shows content of current directory.

- `cp/copy`
Copies file.

- `mv/ren`
Renames a file.

- `ipconfig`
Shows network information.

- `exec`
Runs a file.

- `ftpserver`
Receives data sent from a FTP Client software.
