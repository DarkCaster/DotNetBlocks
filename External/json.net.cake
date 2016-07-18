var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Patch").Does(() =>
{
 var settings=new ProcessSettings()
 {
  WorkingDirectory = new DirectoryPath("json.net"),
  Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../json.net.custom.signkey.patch"),
 };
 
 var result=StartProcess("patch",settings);
 if(result!=0)
  throw new Exception("patch ended with error!");
 
 settings=new ProcessSettings()
 {
  WorkingDirectory = new DirectoryPath("json.net"),
  Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../json.net.nuspec.patch"),
 };
 
 result=StartProcess("patch",settings);
 if(result!=0)
  throw new Exception("patch ended with error!");
});

Task("Build").IsDependentOn("Patch").Does(() =>
{
 if(IsRunningOnWindows())
 {
  MSBuild("json.net/Src/Newtonsoft.Json.sln", settings => settings.SetConfiguration(configuration).WithTarget("Newtonsoft_Json:Rebuild"));
 }
 else
 {
  XBuild("json.net/Src/Newtonsoft.Json.sln", settings => settings.SetConfiguration(configuration).WithTarget("Newtonsoft_Json:Rebuild"));
 }
});

Task("Pack").IsDependentOn("Build").Does(() =>
{
 CreateDirectory("output");
 CreateDirectory("output/net45");
 CopyFiles("json.net/Src/Newtonsoft.Json/bin/Release/Net45/*","output/net45");
 
 var nuGetPackSettings   = new NuGetPackSettings
 {
  BasePath = "output",
 };
 NuGetPack("json.net/Build/Newtonsoft.Json.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);
