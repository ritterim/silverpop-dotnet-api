@echo Off
pushd %~dp0
setlocal enabledelayedexpansion

:: Find the most recent 32bit MSBuild.exe on the system. Also handle x86 operating systems, where %PROGRAMFILES(X86)%
:: is not defined. Always quote the %MSBUILD% value when setting the variable and never quote %MSBUILD% references.
set MSBUILD="%PROGRAMFILES(X86)%\MSBuild\14.0\Bin\amd64\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%PROGRAMFILES(X86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%PROGRAMFILES%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% @set MSBUILD="%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

set CACHED_NUGET=%LOCALAPPDATA%\NuGet\NuGet.exe
if exist %CACHED_NUGET% goto :CopyNuGet

echo Downloading latest version of NuGet.exe...
if not exist %LOCALAPPDATA%\NuGet @md %LOCALAPPDATA%\NuGet
@powershell -NoProfile -ExecutionPolicy Unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:CopyNuGet
if exist .nuget\nuget.exe goto :Build
if not exist .nuget @md .nuget
@copy %CACHED_NUGET% .nuget\nuget.exe > nul

:Build
%MSBUILD% build\Build.proj /m /v:m %* /fl /flp:LogFile=msbuild.log;Verbosity=Detailed /nr:false /nologo

if %ERRORLEVEL% neq 0 goto :BuildFail

:BuildSuccess
echo.
echo *** BUILD SUCCEEDED ***
goto End

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:End
echo.
popd
exit /B %ERRORLEVEL%
