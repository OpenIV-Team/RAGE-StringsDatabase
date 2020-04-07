@echo off

start /B /WAIT compile.cmd build-database.cs build-database.exe /exit
start /B /WAIT compile.cmd sort-strings.cs sort-strings.exe /exit
start /B /WAIT compile.cmd check-string-usage.cs check-string-usage.exe /exit

if "%1"=="/exit" (
  exit
)