@echo off

set help=0
if "%1" == "--help" set help=1
if "%1" == "-h" set help=1
if "%1" == "-*" set help=1

if 1 EQU %help% (
	set args=--showdescription
) else (
	set target=%1
	if "%target%" == "" set target=Default
	set args=--target=%target%
)

@powershell %~dp0build.ps1 %args%
