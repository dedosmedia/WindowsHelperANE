@echo off

REM  Debug/Release
SET CONFIG_NAME=%1
REM x86/x64
SET PLATFORM=%2

SET AIR_PATH=E:\DedosMedia\AIRSDK\WINDOWS\x86\AIR28.0.0.112B\bin\
SET AIR_PATH_64=E:\DedosMedia\AIRSDK\WINDOWS\x64\AIR28.0.0.112B\bin\
IF "%PLATFORM%" == "x64" (SET AIR_PATH=%AIR_PATH_64%)
SET SZIP="C:\Program Files\7-Zip\7z.exe"
SET projectName=WindowsHelperANE
SET pathtome=%~dp0

echo Project: %projectName% Config: %CONFIG_NAME% Platform: %PLATFORM%
echo PathToMe: %pathtome% AIR_SDK: %AIR_PATH% 7ZIP: %SZIP%



echo Making Platforms directories.
IF NOT EXIST %pathtome%platforms mkdir %pathtome%platforms
IF NOT EXIST %pathtome%platforms\win  %pathtome%platforms\win
IF NOT EXIST %pathtome%platforms\win\%PLATFORM% mkdir %pathtome%platforms\win\%PLATFORM%
IF NOT EXIST %pathtome%platforms\win\%PLATFORM%\%CONFIG_NAME% mkdir %pathtome%platforms\win\%PLATFORM%\%CONFIG_NAME%


echo Copying library.swf into place.
call %SZIP% x %pathtome%..\bin\%projectName%.swc -o%pathtome% library.swf -aoa
copy %pathtome%library.swf %pathtome%platforms\win\%PLATFORM%\%CONFIG_NAME%



REM echo Copying SWC into place.
REM echo %pathtome%..\bin\%projectName%.swc
REM copy %pathtome%..\bin\%projectName%.swc %pathtome%

REM echo Extracting files forom SWC.
REM echo %pathtome%%projectName%.swc
REM copy %pathtome%%projectName%.swc %pathtome%%projectName%Extract.swc
REM ren %pathtome%%projectName%Extract.swc %projectName%Extract.zip

REM call %SZIP% e %pathtome%%projectName%Extract.zip -o%pathtome%
REM del %pathtome%%projectName%Extract.zip

REM echo Copying library.swf into place.
REM  copy %pathtome%library.swf %pathtome%platforms\win\%PLATFORM%\%CONFIG_NAME%

echo Copying native libraries into place.
echo Copying %projectName%.dll
copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\%projectName%.dll %pathtome%platforms\win\%PLATFORM%\%CONFIG_NAME%



REM echo Copying %projectName%Lib.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\%projectName%Lib.dll %AIR_PATH%%projectName%Lib.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\%projectName%Lib.pdb %AIR_PATH%%projectName%Lib.pdb
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\%projectName%Lib.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\%projectName%Lib.dll
REM echo Copying FreSharpCore.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharpCore.dll %AIR_PATH%FreSharpCore.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharpCore.pdb %AIR_PATH%FreSharpCore.pdb
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharpCore.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\FreSharpCore.dll
REM echo Copying FreSharp.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharp.dll %AIR_PATH%FreSharp.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharp.pdb %AIR_PATH%FreSharp.pdb
REM copy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\FreSharp.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\FreSharp.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\WindowsHelperLib\bin\%PLATFORM%\%CONFIG_NAME%\DotNetZip.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\DotNetZip.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\WindowsHelperLib\bin\%PLATFORM%\%CONFIG_NAME%\AWSSDK.Core.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\AWSSDK.Core.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\WindowsHelperLib\bin\%PLATFORM%\%CONFIG_NAME%\AWSSDK.S3.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\AWSSDK.S3.dll
REM copy %pathtome%..\..\native_library\win\%projectName%\WindowsHelperLib\bin\%PLATFORM%\%CONFIG_NAME%\Magick.NET-Q16-AnyCPU.dll %pathtome%..\..\c_sharp_libs_%PLATFORM%\Magick.NET-Q16-AnyCPU.dll

echo Copying required dlls 
xcopy %pathtome%..\..\native_library\win\%projectName%\WindowsHelperLib\bin\%PLATFORM%\%CONFIG_NAME%\*.dll %pathtome%..\..\c_sharp_libs_%PLATFORM% /Y

echo Copying dll and pdb for debugging into %AIR_PATH%
xcopy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\*.pdb %AIR_PATH% /Y
xcopy %pathtome%..\..\native_library\win\%projectName%\%PLATFORM%\%CONFIG_NAME%\*.dll %AIR_PATH% /Y

echo Building %CONFIG_NAME%.
call %AIR_PATH%adt.bat -package -target ane %pathtome%%projectName%.ane %pathtome%extension_win.xml -swc %pathtome%..\bin\%projectName%.swc ^
-platform Windows-x86 -C %pathtome%platforms\win\x86\%CONFIG_NAME% %projectName%.dll library.swf ^
-platform Windows-x86-64 -C %pathtome%platforms\win\x64\%CONFIG_NAME% %projectName%.dll library.swf ^
-platform default -C %pathtome% "library.swf"

REM call DEL /F /Q /A %pathtome%%projectName%.swc
call DEL /F /Q /A %pathtome%library.swf
REM call DEL /F /Q /A %pathtome%catalog.xml

echo ---=== BUILD ANE DONE ===---


call %pathtome%build-myapp.bat
