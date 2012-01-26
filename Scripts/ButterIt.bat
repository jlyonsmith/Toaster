setlocal
call SetSlnRoot.bat
set CONFIGURATION=Debug
set TESTDIR=%SOLUTIONROOT%\TestDeployment
md %TESTDIR%
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.exe %TESTDIR%
copy %SOLUTIONROOT%\Toast\bin\%CONFIGURATION%\Toast.pdb %TESTDIR%
copy %SOLUTIONROOT%\Butter\bin\%CONFIGURATION%\Butter.exe %TESTDIR%
copy %SOLUTIONROOT%\Butter\bin\%CONFIGURATION%\Butter.pdb %TESTDIR%
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.dll %TESTDIR%
copy %SOLUTIONROOT%\ToolBelt\bin\%CONFIGURATION%\ToolBelt.pdb %TESTDIR%
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.dll %TESTDIR%
copy %SOLUTIONROOT%\Toaster\bin\%CONFIGURATION%\Toaster.pdb %TESTDIR%
%TESTDIR%\butter.exe %SOLUTIONROOT%\Tests\ToolBelt.UnitTests\bin\%CONFIGURATION%\ToolBelt.UnitTests.dll -dd:%SOLUTIONROOT%\TestDeployment  -o:%TESTDIR%\Test.testresults -cdi:%SOLUTIONROOT%TGTServiceTests\bin\Debug\ToolBelt.dll
endlocal