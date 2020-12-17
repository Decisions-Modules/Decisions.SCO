@echo off
@echo Building with MsBuild
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe" build.proj
IF NOT errorlevel 0 GOTO :error
copy ..\..\output\modules\Decisions.SCO.zip "c:\Program Files\Decisions\Decisions Services Manager\modules" /Y
net stop "Service Host Manager Watcher"
net stop "Service Host Manager"
net start "Service Host Manager"