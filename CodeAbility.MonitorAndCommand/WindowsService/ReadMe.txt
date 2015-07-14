To install your service manually

On the Windows Start menu or Start screen, choose Visual Studio , Visual Studio Tools, Developer Command Prompt.
A Visual Studio command prompt appears.
Access the directory where your project's compiled executable file is located.
Run InstallUtil.exe from the command prompt with your project's executable as a parameter: installutil <yourproject>.exe
If you’re using the Visual Studio command prompt, InstallUtil.exe should be on the system path. 
If not, you can add it to the path, or use the fully qualified path to invoke it. 
This tool is installed with the .NET Framework, and its path is %WINDIR%\Microsoft.NET\Framework[64]\<framework_version>. 
For example, for the 32-bit version of the .NET Framework 4 or 4.5.*, if your Windows installation directory is C:\Windows, the path is C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe. For the 64-bit version of the .NET Framework 4 or 4.5.*, the default path is C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe.

To uninstall your service manually

On the Windows Start menu or Start screen, choose Visual Studio, Visual Studio Tools, Developer Command Prompt.
A Visual Studio command prompt appears.
Run InstallUtil.exe from the command prompt with your project's output as a parameter: installutil /u <yourproject>.exe
Sometimes, after the executable for a service is deleted, the service might still be present in the registry. In that case, use the command sc delete to remove the entry for the service from the registry.

Source : http://msdn.microsoft.com/en-us/library/sd8zc8ha(v=vs.110).aspx