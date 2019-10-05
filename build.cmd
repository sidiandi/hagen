@echo off
rem Do not modify
rem Bootstrapper script of https://github.com/sidiandi/Amg.Build
setlocal
set AmgBuildTargetFramework=netcoreapp3.0
set name=%~n0
set exe=%~dp0%name%\bin\Debug\%AmgBuildTargetFramework%\%name%.exe
set project=%~dp0%name%\%name%.csproj
if exist %exe% (%exe% %*) else (dotnet run --project %project% -- %*)
