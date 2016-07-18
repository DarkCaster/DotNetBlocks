var target = Argument("target", "Default");

Task("Patch").Does(() =>
{
 var settings=new ProcessSettings()
 {
  WorkingDirectory = new DirectoryPath("nunit"),
  Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../nunit-nuspec.patch"),
 };
 var result=StartProcess("patch",settings);
 if(result!=0)
  throw new Exception("patch ended with error!");

 if(IsRunningOnWindows())
 {
  settings=new ProcessSettings()
  {
   WorkingDirectory = new DirectoryPath("nunit"),
   Arguments = new ProcessArgumentBuilder().Append("-p1").Append("-i").Append("../nunit-nuspec-net45only.patch"),
  };
  var result2=StartProcess("patch",settings);
  if(result2!=0)
   throw new Exception("patch ended with error!");
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

