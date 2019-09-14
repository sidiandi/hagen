@echo off
setlocal EnableDelayedExpansion
set buildDll=%~dp0%~n0\bin\Debug\netcoreapp2.2\build.dll
set exitCodeRebuildRequired=2

mkdir %buildDll%\.. 2>nul
echo startup time > %buildDll%.startup

if exist %buildDll% (
    dotnet %buildDll% %*
    set buildScriptExitCode=!errorlevel!
    if !buildScriptExitCode! equ %exitCodeRebuildRequired% (
        call :rebuild %*
    )
    exit /b !buildScriptExitCode!
) else (
    call :rebuild %*
)
goto :eof

:rebuild
    dotnet run --force -vd --project %~dp0%~n0 -- --ignore-clean %*
	set buildScriptExitCode=!errorlevel!
	exit /b !buildScriptExitCode!
