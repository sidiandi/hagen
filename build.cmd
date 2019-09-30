rem @echo off
rem Do not modify below this line
rem Bootstrapper script of https://github.com/sidiandi/Amg.Build
setlocal
set AmgBuildTargetFramework=netcoreapp3.0
set name=%~n0
set dll=%~dp0%name%\bin\Debug\%AmgBuildTargetFramework%\%name%.dll
set project=%~dp0%name%\%name%.csproj
if exist %dll% ( dotnet %dll% %* ) else ( dotnet run --project %project% - %* )
