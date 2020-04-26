cd ..\tools\
start /B /WAIT ..\tools\compile-all-tools.cmd /exit
cd ..\RDR2\
start /B /WAIT ..\tools\check-string-usage.exe AudioTracks AWC %1
