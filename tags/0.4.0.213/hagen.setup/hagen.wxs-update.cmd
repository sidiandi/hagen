set STAGE=%TEMP%\hagen
set WIXBIN=%ProgramFiles(x86)%\WiX Toolset v3.8\bin
del /s /q %STAGE%
robocopy /mir /s C:\build\hagen\Release-AnyCPU %STAGE% /xf *.xml /xf *.pdb /xd test
"%WIXBIN%\heat" dir %STAGE% -cg ProductComponents -ag -sreg -sfrag -o %~dp0hagen.wxs -var var.hagen.TargetDir -dr INSTALLFOLDER
