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

Task("Patch").Does(() =>
{
  Patch("json.net","../json.net.build_and_sign.patch",1);
  Patch("json.net","../json.net.disable_extra_projects.patch",1);
});

Task("Build").IsDependentOn("Patch").Does(() =>
{
	MSBuild("json.net/Src/Newtonsoft.Json.sln", settings =>
	{
		settings.SetConfiguration(configuration).WithTarget("Restore").WithTarget("Newtonsoft_Json:Rebuild");
		if(IsRunningOnUnix())
			settings.ToolPath = "/usr/bin/msbuild";
	});
	if(SNTest("json.net/Src/Newtonsoft.Json/bin/Release/net45/Newtonsoft.Json.dll"))
		SNSign("json.net/Src/Newtonsoft.Json/bin/Release/net45/Newtonsoft.Json.dll","json.net.custom.signkey.snk");
});

Task("Pack").IsDependentOn("Build").Does(() =>
{
  CreateDirectory("output");
  CreateDirectory("output/net45");
  CopyFiles("json.net/Src/Newtonsoft.Json/bin/Release/net45/*","output/net45");
  var nuGetPackSettings = new NuGetPackSettings { BasePath = "output", };
  NuGetPack("json.net.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Pack");
RunTarget(target);
