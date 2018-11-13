var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

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

private bool SNTest(string assembly)
{
  var settings=new ProcessSettings()
  {
    WorkingDirectory = new DirectoryPath("."),
    Arguments = new ProcessArgumentBuilder().Append("-v").Append(assembly),
  };
  if(StartProcess("sn",settings)==1)
    return true;
  return false;
}

private void SNSign(string assembly, string key)
{
  var settings=new ProcessSettings()
  {
    WorkingDirectory = new DirectoryPath("."),
    Arguments = new ProcessArgumentBuilder().Append("-R").Append(assembly).Append(key),
  };
  if(StartProcess("sn",settings)!=0)
    throw new Exception("full-sign failed!");
}

private void MSBuild_Linux(string solution)
{
  var settings=new ProcessSettings()
  {
    WorkingDirectory = new DirectoryPath("."),
    Arguments = new ProcessArgumentBuilder().Append(solution).Append("/t:Rebuild").Append("/p:Configuration=Release").Append("/p:BuildProjectReferences=false"),
  };
  if(StartProcess("msbuild",settings)!=0)
    throw new Exception("msbuild failed!");
}

Task("Restore-NuGet-Packages").Does(() => { NuGetRestore("msgpack/MsgPack.sln"); });

Task("Patch").Does(() => { Patch("msgpack","../msgpack-nuspec.patch",1); });

Task("Build").IsDependentOn("Patch").Does(() =>
{
  if(IsRunningOnWindows())
    MSBuild("msgpack/MsgPack.sln", settings => settings.SetConfiguration(configuration).WithTarget("src\\_NET45\\MsgPack_Net45:Rebuild"));
  else
  {
    MSBuild_Linux("msgpack/MsgPack.sln");
    //perform full-sign
    if(SNTest("msgpack/bin/net45/MsgPack.dll"))
      SNSign("msgpack/bin/net45/MsgPack.dll","msgpack/MsgPack.snk");
    if(SNTest("msgpack/bin/net46/MsgPack.dll"))
      SNSign("msgpack/bin/net46/MsgPack.dll","msgpack/MsgPack.snk");
  }
});

Task("Pack").IsDependentOn("Build").Does(() =>
{
  var nuGetPackSettings = new NuGetPackSettings
  {
    BasePath = "msgpack/bin",
  };
  NuGetPack("msgpack/MsgPack.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);
