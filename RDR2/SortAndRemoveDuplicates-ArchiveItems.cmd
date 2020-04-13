cd ..\tools\
start /B /WAIT ..\tools\compile-all-tools.cmd /exit
cd ..\RDR2\
start /B /WAIT ..\tools\sort-strings.exe ArchiveItems Default
