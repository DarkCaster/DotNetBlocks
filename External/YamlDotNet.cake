var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release-Signed");

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

Task("Patch").Does(() =>
{
  Patch("YamlDotNet","../YamlDotNet.nuspec.patch",1);
});

Task("Build").IsDependentOn("Patch").Does(() =>
{
  MSBuild("YamlDotNet/YamlDotNet.sln", settings =>
	{
		settings.SetConfiguration(configuration).WithTarget("YamlDotNet:Rebuild");
		if(IsRunningOnUnix())
            settings.ToolPath = "/usr/bin/msbuild";
	});
  if(IsRunningOnUnix() && SNTest("YamlDotNet/YamlDotNet/bin/Release-Signed/net35/YamlDotNet.dll"))
    SNSign("YamlDotNet/YamlDotNet/bin/Release-Signed/net35/YamlDotNet.dll","YamlDotNet/YamlDotNet.snk");
});

Task("Pack").IsDependentOn("Build").Does(() =>
{
  CreateDirectory("output");
  CreateDirectory("output/net35");
  CopyFiles("YamlDotNet/YamlDotNet/bin/Release-Signed/net35/*","output/net35");
  var nuGetPackSettings = new NuGetPackSettings { BasePath = "output", };
  NuGetPack("YamlDotNet/YamlDotNet/YamlDotNet.Signed.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);
