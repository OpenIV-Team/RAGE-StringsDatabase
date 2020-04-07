@echo off

set sourceFileCS=%1
set targetFileEXE=%2

echo Compiling %sourceFileCS% as %targetFileEXE%...

if exist %targetFileEXE% (
  echo Target file %targetFileEXE% already exist
  goto :compare_time
) else (
  goto :compile
)

:compare_time
echo Compraing timestamp for %sourceFileCS% and %targetFileEXE%...
for %%A in (%sourceFileCS%) do (
  set fileDateCS=%%~tA
)

for %%A in (%targetFileEXE%) do (
  set fileDateEXE=%%~tA
)

if "%fileDateCS%"=="%fileDateEXE%" ( goto :end )

for /F %%i in ('DIR /B /O:D %sourceFileCS% %targetFileEXE%') DO SET NEWEST=%%i

if "%NEWEST%"=="%sourceFileCS%" (
  echo Source file %sourceFileCS% is newer...
  goto :compile
) else (
  echo Target file %targetFileEXE% is newer, no reason to recompile
  goto :end
)

:compile
echo Compiling...
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:%targetFileEXE% %sourceFileCS% tools-shared.cs

:end
echo.
if "%3"=="/exit" (
  exit
)
