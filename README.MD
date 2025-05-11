# MDRG (My Dystopian Robot Girlfriend) Analyzer - Cross Platform Branch

## This does **not** have Save Editing!

Save editing is (currently) restricted to only the [Main Branch](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer), which is only native to Windows. This program is only for reading the save files, not editing them. If you want to edit the save files, please use the main branch!

## Usage

More expert, do NOT use unless you know your way around commandline.

### Minimum requirements

- CPU: Any
- RAM: Any
- GPU: Any
- Space: N/A
- Screen: N/A

- Windows (Mainly due to .NET Core 9.0): **Windows 11 (24H2, 23H2, 22H2 Ent/Edu)** or **Windows 10 (22H2+)**
- .NET Core 9.0+

You need very likely need the most recent [.NET Core 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
installed on your PC. If you installed Windows 10 after at least the 26th of March, 2025, you might have this already
installed.

## Linux and Macintosh/MAC

Due to this program being made in C#, it is possible to run it on Linux and Mac. However, those platforms are not directly supported, as I use neither of them. If you want to run it on Linux or Mac, you at least need to install the .NET Core 9.0 runtime.

## Links
[Click here to get to the GitHub repository of the main program.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer)

[Click here to get the latest version of the program.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/releases/latest)

[Click here to report bugs, but **make sure** you specify you are using the COMMANDLINE or CROSS PLATFORM version.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/issues)

[Click here to suggest things.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/discussions/categories/ideas)

## Frequently Asked Questions

### I built this myself and get errors! (Visual Studio 2022)

First off, have you modified anything? If yes, check it yourself. However, the following might fix your problem too:

**I haven't modified anything, but it still gives an error!**

First off, check:

\- Are you running in Debug?

If yes, then follow this:

1. On the top bar, click on **<ins>D</ins>ebug**
2. Click on **Options...**
3. Check **Enable Just My Code**
4. Rebuild and run

This should fix some of the errors that happen during startup or runtime.

### What dependencies does this program have?

To find this program's `using`, check the file `Form1.cs` and look at the top.