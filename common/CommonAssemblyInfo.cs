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

[assembly: ComVisible(false)]
