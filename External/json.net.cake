var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Patch").Does(() =>
{
 var settings=new ProcessSettings()
 {
  WorkingDirectory = new DirectoryPath("json.net"),
  Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../json.net.custom.signkey.patch"),
 };
 
 StartProcess("patch",settings);
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
 var nuGetPackSettings   = new NuGetPackSettings
 {
  BasePath = "json.net/bin",
 };
 NuGetPack("json.net/json.net.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Build");
RunTarget(target);
