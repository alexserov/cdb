@echo off
cls
mkdir temp >nul

if "%1"=="" (
    echo "\invoke.bat FileName.exe [x86|x64] Executable.[bat|exe] [double-quoted executable parameter]"
    goto end
)

set exeName=%1
set architecture=%2
set executable=%3

if "%architecture%"=="x86" goto archOK
if "%architecture%"=="x64" goto archOK

echo "Parameter #2 should be x86 or x64"
goto end
:archOK

regedit /e temp/cached.reg "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps"

reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v DumpCount /d 15 >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_EXPAND_SZ  /v DumpFolder /d "%cd%\temp\dump" >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v DumpType /d 0 >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v CustomDumpFlags /d 4398 >nul

call %executable% %4

FOR %%F IN ("%cd%\temp\dump\*.*") DO (
 set filename=%%F
 goto debug
)
goto regcleanup

:debug
echo. .loadby sos clr; .symfix; !threads; !sym noisy; .logopen "%cd%\temp\callstack.txt"; !EEStack -EE; .logclose; qd>temp/dbg.script
"%cd%\%architecture%\cdb.exe" -z "%filename%" -c "$<%cd%\temp\dbg.script" >nul

powershell .\reduceCallstack.ps1 .\temp\callstack.txt

type "%cd%\temp\callstack.txt"

:regcleanup
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps" /f >nul
regedit /s temp/cached.reg

del /s /q temp\* >nul
rmdir /s /q temp >nul

:end