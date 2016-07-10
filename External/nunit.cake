var target = Argument("target", "Default");

Task("Patch").Does(() =>
{
 var settings=new ProcessSettings()
 {
  WorkingDirectory = new DirectoryPath("nunit"),
  Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../nunit-nuspec.patch"),
 };
 StartProcess("patch",settings);

 if(IsRunningOnWindows())
 {
  settings=new ProcessSettings()
  {
   WorkingDirectory = new DirectoryPath("nunit"),
   Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../nunit-nuspec-net4only.patch"),
  };
  StartProcess("patch",settings);
 }
});

Task("Build").IsDependentOn("Patch").Does(() =>
{
 var args=new System.Collections.Generic.Dictionary<string,string>();
 args.Add("configuration","Release");
 CakeExecuteScript("nunit/build.cake", new CakeSettings{ Arguments = args });
});

Task("Pack").IsDependentOn("Build").Does(() =>
{
 var args=new System.Collections.Generic.Dictionary<string,string>();
 args.Add("configuration","Release");
 args.Add("target","PackageNuGet");
 CakeExecuteScript("nunit/build.cake", new CakeSettings{ Arguments = args });
 CreateDirectory("packages");
 CopyFiles("nunit/package/*.nupkg","packages");
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);

