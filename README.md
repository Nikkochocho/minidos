# minidos

## Table of contents
- [About](#about)
- [Prerequisites](#prerequisites)
- [HowTo](#howto)

## About
`minidos` is a small DOS implementation, whose functions are quite similar to those of the original MS-DOS, with the difference of covering not only DOS commands but also Linux commands. The project was made using the C# language on Cosmos (C# Open Source Managed Operating System).

## Prerequisites
In order to use the minidos application, it is necessary to obtain the following resources:
- [Cosmos OS SDK](https://github.com/CosmosOS/Cosmos).
- A virtual machine to get the operating system running. VMware Player is recommended. You can obtain the setup installation [here].(https://www.vmware.com/products/desktop-hypervisor/workstation-and-fusion).
- `minidos` also has a dependency on the project [NLua](https://github.com/NLua/NLua) to get program running on Lua executables. You may acquire the NLua package through NuGet on [here](https://www.nuget.org/packages/NLua).

## HowTo
To get the software runnning, you must first install the latest version of Visual Studio and a .NET Core 8.0 or higher. Then, you will also need to acquire a virtual machine (in this example, we will be using VMware Player, but Cosmos also support Virtual Box and VMware Workstation). Lastly, it is necessary to install the Cosmos OS SDK and the NLua package. Both are linked in the [Prerequisites](#prerequisites) subsection.

