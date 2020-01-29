.loadby sos clr;
.symfix;
!threads;
!sym noisy;
.logopen C:\\t\\c.txt;
!EEStack -EE; .logclose;
.load dbgextensions.dll;
!executescript "powershell Set-ExecutionPolicy RemoteSigned";
!executescript "powershell C:\\t\\r.ps1 C:\\t\\c.txt"; 
qq