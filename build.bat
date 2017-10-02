rem %1 : build target. Default: Release

set Target=%1
if "%Target%" == "" (
	set Target=Release
)

set msbuild="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
set SourceDir=%~dp0.
call :file_name_from_path DirName %SourceDir%
%msbuild% "%SourceDir%\build\Bootstrap.proj" /p:BuildTarget=%Target%
goto :eof

:file_name_from_path <resultVar> <pathVar>
(
    set "%~1=%~nx2"
    exit /b
)

:eof
