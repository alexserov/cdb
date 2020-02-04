.loadby sos clr;
.symfix;
!threads;
!sym noisy;
.logopen {tempDirectory}\callstack.txt;
!EEStack -EE; 
.logclose;
.load dbgextensions.dll;
!executescript "powershell Set-ExecutionPolicy -Scope CurrentUser RemoteSigned";
!executescript "powershell . {scriptDirectory}\\reduceCallStack.ps1 {tempDirectory}\\callstack.txt"; 
!executescript "powershell . {scriptDirectory}\\publish.ps1 {tempDirectory}\\callstack.txt"; 
qq