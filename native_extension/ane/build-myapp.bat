REM Get the path to the script and trim to get the directory.
@echo off
SET AIR_PATH="C:\Users\Diego\Desktop\AIR_SDK_27.124\bin\"
SET AIR_PATH_64="C:\Users\Diego\Desktop\AIRSDK_26.123_x64\bin\"

echo Copying Native Extension to AIRSDK subfolder (to make it easy to package my App using this ANE)
copy %pathtome%%projectName%.ane %AIR_PATH%\Keshot\WindowsHelperANE\ane\%projectName%.ane
copy %pathtome%%projectName%.ane %AIR_PATH_64%\Keshot\WindowsHelperANE\ane\%projectName%.ane

echo Copying required dlls
xcopy %pathtome%..\..\c_sharp_libs_x86 %AIR_PATH%\Keshot\WindowsHelperANE\dll\ /Y
xcopy %pathtome%..\..\c_sharp_libs_x64 %AIR_PATH_64%\Keshot\WindowsHelperANE\dll\ /Y

cd /d %AIR_PATH%\Keshot\
call Package.bat

cd /d %AIR_PATH_64%\Keshot\
call Package.bat

echo DONE Packaging x86 and x64