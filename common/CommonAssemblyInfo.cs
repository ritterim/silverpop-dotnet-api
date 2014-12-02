using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("Silverpop API")]
[assembly: AssemblyCopyright("Copyright © 2014 Ritter Insurance Marketing")]
[assembly: AssemblyCompany("Ritter Insurance Marketing")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.0.1.0")]
[assembly: AssemblyFileVersion("0.0.1.0")]

[assembly: ComVisible(false)]
