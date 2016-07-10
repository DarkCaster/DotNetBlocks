@echo off
set ScriptRoot=%~dp0

"%ScriptRoot%\External\DotNetBuildTools\prepare-and-build.bat" "%ScriptRoot%\External"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%

copy "%ScriptRoot%\External\DotNetBuildTools\dist\extra\NuGet.Config" "%ScriptRoot%\NuGet.Config"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%

