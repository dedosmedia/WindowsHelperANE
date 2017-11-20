@echo off
SET CUSTOM_SETUP="E:\Dropbox\DedosMedia\keshot\Keshot-PhotoBooth\Custom-Setup\"

echo Copying Native Extension to Custom Setup (to make it easy to package my App using this ANE)
copy %pathtome%%projectName%.ane %CUSTOM_SETUP%\Keshot\%projectName%\ane\%projectName%.ane

echo Copying required dlls
xcopy %pathtome%..\..\c_sharp_libs_%PLATFORM% %CUSTOM_SETUP%\Keshot\%projectName%\dll\ /Y

echo Copying unzipped ANE for adl debug
call %SZIP% x %pathtome%%projectName%.ane -o%CUSTOM_SETUP%\Keshot\%projectName%\unzip\%projectName%.ane -aoa
xcopy %pathtome%..\..\c_sharp_libs_%PLATFORM% %CUSTOM_SETUP%\Keshot\%projectName%\dll\ /Y

cd /d %CUSTOM_SETUP%\Keshot\

echo Packaing myapp %AIR_PATH%
call %AIR_PATH%adt.bat -package -storetype PKCS12 -keystore cert.p12 -storepass aristi82 -tsa none -target bundle DedosMedia Keshot-AIR-Windows-app.xml -extdir %projectName%/ane Keshot_windows.swf -C %projectName%/dll . -C data .


echo ---=== CAPTIVE INSTALLER %PLATFORM% DONE ===---
