using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Delta Shell")]
[assembly : AssemblyDescription("")]
[assembly : AssemblyConfiguration("")]
[assembly : AssemblyCompany("Deltares")]
[assembly: AssemblyProduct("Delta Shell")]
[assembly: AssemblyCopyright("Copyright � Deltares 2010")]
[assembly : AssemblyTrademark("")]
[assembly : AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly : ComVisible(false)]

[assembly: CLSCompliant(true)] //fxcop prerequisite
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)] //fxcop prerequisite

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly : Guid("54e200fe-6297-4bfe-b449-67f3d4395144")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.0.9.0")]
[assembly: AssemblyFileVersion("0.0.9.0")]
