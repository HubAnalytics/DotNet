using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !DNXCORE50
using System.Web;
#endif

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MicroserviceAnalytics.EF6")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MicroserviceAnalytics.EF6")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7ee03344-f353-4f8b-af13-df1f548613b0")]

#if !DNXCORE50
[assembly: PreApplicationStartMethod(typeof(MicroserviceAnalytics.EF6.MicroserviceAnalytics), "Attach")]
#endif