@echo off
setlocal
call SetSlnRoot.bat
set CONFIGURATION=Debug
set TESTDIR=%SOLUTIONROOT%\TestDeployment
md %TESTDIR%
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.exe %TESTDIR%
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.pdb %TESTDIR%
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.dll %TESTDIR%
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.pdb %TESTDIR%
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.dll %TESTDIR%
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.pdb %TESTDIR%
%TESTDIR%\toast.exe %SOLUTIONROOT%\Tests\ToolBelt.UnitTests\bin\%CONFIGURATION%\ToolBelt.UnitTests.dll -dd:%TESTDIR% -o:%TESTDIR%\TestResults.testresults %*
endlocal