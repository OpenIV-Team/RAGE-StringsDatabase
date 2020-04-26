cd ..\tools\
start /B /WAIT compile-all-tools.cmd /exit
cd ..\RDR2\
start /B /WAIT ..\tools\build-database.exe OPENIV TextKeys Default TextKeys
