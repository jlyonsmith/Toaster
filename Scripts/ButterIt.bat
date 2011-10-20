setlocal
set SOLUTIONROOT=C:\INRIX\ConnectedServices\MAIN\Inrix.CS\Customer\TGT\
butter.exe %SOLUTIONROOT%TGTServiceTests\bin\Debug\TGTServiceTests.dll /dd:%SOLUTIONROOT%TestDeployment /di:%SOLUTIONROOT%TGTServiceTests\bin\Debug\ToolBelt.dll /di:%SOLUTIONROOT%tgtservicetests\bin\Debug\BouncyCastle.CryptoExt.dll /cdi:%SOLUTIONROOT%TGTServiceTests\bin\Debug\TGTMockService.dll  /di:%SOLUTIONROOT%TGTServiceTests\bin\Debug\MockFramework.dll
endlocal