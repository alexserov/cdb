@echo off
cls

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

set %cdtemp%=%cd%\temp
if not "{tempDirectory}"=="%cd%\temp" set %cdtemp%={tempDirectory}
set %cdscript%=%cd%\cdscripts
if not "{scriptDirectory}"=="%cd%\cdscripts" set %cdscript%={scriptDirectory}
set %cdcdb%=%cd%\%architecture%
if not "{cdbDirectory}"=="%cd%\%architecture%" set %cdcdb%={cdbDirectory}

mkdir %cdtemp%\ >nul

regedit /e %cdtemp%\cached.reg "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps"

reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v DumpCount /d 15 >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_EXPAND_SZ  /v DumpFolder /d "%cdtemp%\dump" >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v DumpType /d 0 >nul
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps\%1" /f /t REG_DWORD  /v CustomDumpFlags /d 4398 >nul

call %executable% %4

FOR %%F IN ("%cdtemp%\dump\*.*") DO (
 set filename=%%F
 goto debug
)
goto regcleanup

:debug
echo. .loadby sos clr; .symfix; !threads; !sym noisy; .logopen "%cdtemp%\callstack.txt"; !EEStack -EE; .logclose; qd>%cdtemp%\dbg.script
"%cdcdb%\cdb.exe" -z "%filename%" -c "$<%cdtemp%\dbg.script" >nul

powershell %cdscript%\reduceCallstack.ps1 %cdtemp%\callstack.txt

type "%cdtemp%\callstack.txt"

:regcleanup
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps" /f >nul
regedit /s %cdtemp%/cached.reg

del /s /q %cdtemp%\* >nul
rmdir /s /q %cdtemp% >nul

:end