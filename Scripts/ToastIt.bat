@echo off
setlocal
set SOLUTIONROOT=c:\inrix\thirdparty\toaster\1.3
set CONFIGURATION=Debug
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.exe TestDeployment
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.pdb TestDeployment
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.dll TestDeployment
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.pdb TestDeployment
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.dll TestDeployment
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.pdb TestDeployment
TestDeployment\toast.exe %SOLUTIONROOT%\Tests\ToolBelt.UnitTests\bin\%CONFIGURATION%\ToolBelt.UnitTests.dll /dd:%SOLUTIONROOT%\TestDeployment %*
endlocal