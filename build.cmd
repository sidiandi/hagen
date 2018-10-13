@echo off
set args=%*
powershell %~dp0build.ps1 %args%
