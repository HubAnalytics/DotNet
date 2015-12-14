using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !DNXCORE50
using System.Web;
#endif

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MicroserviceAnalytics.Ado")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MicroserviceAnalytics.Ado")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("60bd1293-ccc2-40c0-bc1e-f1cc5f7190fd")]

#if !DNXCORE50
[assembly: PreApplicationStartMethod(typeof(MicroserviceAnalytics.Ado.MicroserviceAnalytics), "Attach")]
#endif