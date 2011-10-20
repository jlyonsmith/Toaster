@echo off
setlocal
set TargetDir=%ProgramW6432%\Toaster\
set Config=Release
xcopy /dy Toast\bin\%Config%\Toast.exe "%TargetDir%"
xcopy /dy Toast\bin\%Config%\ToolBelt.dll "%TargetDir%"
xcopy /dy Toast\bin\%Config%\Toaster.dll "%TargetDir%"
xcopy /dy Butter\bin\%Config%\Butter.exe "%TargetDir%"
xcopy /dy Crumb\bin\%Config%\Crumb.exe "%TargetDir%"
endlocal