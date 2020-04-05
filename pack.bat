@ECHO off
ECHO Linking files in %1 to %1\Script.cs
IF EXIST %1\Script.cs (
  ECHO Moving %1\Script.cs to %1\.Script.old.cs
  MOVE %1\Script.cs %1\.Script.old.cs
)
ECHO Found files:
FOR /f %%f in ('DIR /B /S %1\_*.cs') do (
  REM print filename to stdout
  ECHO %%f
  (
    REM append file, starting and ending with filename in comments
    ECHO //
    ECHO // XXX %%f XXX
    ECHO //
    REM hacky way to add a newline
    ECHO(
    TYPE %%f
    ECHO(
  ) >> %1\Script.cs
)
