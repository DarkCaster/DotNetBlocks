@echo off
REM Use random variable name, so calling another bat file will not spoil it.
set aafc2aac361f=%~dp0

call "%aafc2aac361f%\External\DotNetBuildTools\prepare-and-build.bat" "%aafc2aac361f%\External"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%

copy /Y "%aafc2aac361f%\External\DotNetBuildTools\dist\extra\NuGet.Config" "%aafc2aac361f%\NuGet.Config"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%
