var target = Argument("target", "Default");

private void Patch(string workDir, string patch, int p)
{
  var settings=new ProcessSettings()
  {
    WorkingDirectory = new DirectoryPath(workDir),
    Arguments = new ProcessArgumentBuilder().Append("-p"+p.ToString()).Append("-i").Append(patch),
  };
  if(StartProcess("patch",settings)!=0)
    throw new Exception("patch failed!");
}

Task("Patch").Does(() =>
{
  Patch("nunit","../nunit-net45-only.patch",1);
  Patch("nunit","../nunit-net45-only-nuspec.patch",1);
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
  args.Add("target","Package");
  CakeExecuteScript("nunit/build.cake", new CakeSettings{ Arguments = args });
  CreateDirectory("packages");
  CopyFiles("nunit/package/*.nupkg","packages");
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);
