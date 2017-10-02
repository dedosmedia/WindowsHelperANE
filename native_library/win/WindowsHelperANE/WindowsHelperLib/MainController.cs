using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TuaRua.FreSharp;
using static WindowsHelperLib.ShowWindowCommands;
using FREObject = System.IntPtr;
using FREContext = System.IntPtr;
using Hwnd = System.IntPtr;
using System.Text;
using Ionic.Zip;
using System.IO;

using Amazon.S3;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace WindowsHelperLib {
    public class MainController : FreSharpController {
        // ReSharper disable once NotAccessedField.Local
        private Hwnd _airWindow;

        private Hwnd _foundWindow;
        private readonly Dictionary<string, DisplayDevice> _displayDeviceMap = new Dictionary<string, DisplayDevice>();
        private bool _isHotKeyManagerRegistered;
        private AmazonS3Client client;

        

        public string[] GetFunctions() {
            FunctionsDict =
                new Dictionary<string, Func<FREObject, uint, FREObject[], FREObject>> {
                    {"init", InitController},
                    {"findWindowByTitle", FindWindowByTitle},
                    {"showWindow", ShowWindow},
                    {"hideWindow", HideWindow},
                    {"setForegroundWindow", SetForegroundWindow},
                    {"getDisplayDevices", GetDisplayDevices},
                    {"setDisplayResolution", SetDisplayResolution},
                    {"restartApp", RestartApp},
                    {"registerHotKey", RegisterHotKey},
                    {"unregisterHotKey", UnregisterHotKey},
                    {"getNumLogicalProcessors",GetNumLogicalProcessors},
					
					  
                    {"findTaskBar", FindTaskBar},
                    {"isProgramRunning", IsProgramRunning},
                    {"unzipFile", UnzipFile},
                    {"testCV", TestCV},
                    {"test", Test},
                    {"makeTopMostWindow", MakeTopMostWindow},
                    {"makeNoTopMostWindow", MakeNoTopMostWindow},
                    {"makeBottomWindow", MakeBottomWindow},
                    {"resizeWindow", ResizeWindow},
                    {"readIniValue", ReadIniValue}



                };

            return FunctionsDict.Select(kvp => kvp.Key).ToArray();
        }

        public FREObject NotImplemented(FREContext ctx, uint argc, FREObject[] argv) {
            return FREObject.Zero;
        }

        public FREObject InitController(FREContext ctx, uint argc, FREObject[] argv) {
            _airWindow = Process.GetCurrentProcess().MainWindowHandle;
            return FREObject.Zero;
        }

        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e) {
            var key = Convert.ToInt32(e.Key);
            var modifier = Convert.ToInt32(e.Modifiers);
            var sf = $"{{\"key\": {key}, \"modifier\": {modifier}}}";
            Context.SendEvent("ON_HOT_KEY", sf);
            /*
             Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
            */
        }

        public FREObject RegisterHotKey(FREContext ctx, uint argc, FREObject[] argv) {
            var key = Convert.ToInt32(new FreObjectSharp(argv[0]).Value);
            var modifier = Convert.ToInt32(new FreObjectSharp(argv[1]).Value);
            var id = HotKeyManager.RegisterHotKey((Keys) key, (KeyModifiers) modifier);
            if (!_isHotKeyManagerRegistered) {
                HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
            }
            _isHotKeyManagerRegistered = true;
            return new FreObjectSharp(id).RawValue;
        }

        public FREObject UnregisterHotKey(FREContext ctx, uint argc, FREObject[] argv) {
            var id = Convert.ToInt32(new FreObjectSharp(argv[0]).Value);
            HotKeyManager.UnregisterHotKey(id);
            return FREObject.Zero;
        }

        public FREObject GetNumLogicalProcessors(FREContext ctx, uint argc, FREObject[] argv) {
            return new FreObjectSharp(Environment.ProcessorCount).RawValue;
        }

        public FREObject FindWindowByTitle(FREContext ctx, uint argc, FREObject[] argv) {
            var searchTerm = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            foreach (var pList in Process.GetProcesses()) {
                if (!string.IsNullOrEmpty(searchTerm) && !pList.MainWindowTitle.Contains(searchTerm)) continue;
                _foundWindow = pList.MainWindowHandle;
                return new FreObjectSharp(pList.MainWindowTitle).RawValue;
            }
            return FREObject.Zero;
        }

        public FREObject ShowWindow(FREContext ctx, uint argc, FREObject[] argv) {
            var maximise = (bool) new FreObjectSharp(argv[0]).Value;
            if (WinApi.IsWindow(_foundWindow)) {
                WinApi.ShowWindow(_foundWindow, maximise ? SW_SHOWMAXIMIZED : SW_RESTORE);
            }
            return FREObject.Zero;
        }

        public FREObject HideWindow(FREContext ctx, uint argc, FREObject[] argv) {
            if (WinApi.IsWindow(_foundWindow)) {
                WinApi.ShowWindow(_foundWindow, SW_HIDE);
            }
            return FREObject.Zero;
        }

        public FREObject SetForegroundWindow(FREContext ctx, uint argc, FREObject[] argv) {
            if (WinApi.IsWindow(_foundWindow)) {
                WinApi.SetForegroundWindow(_foundWindow);
            }
            return FREObject.Zero;
        }

        private struct DisplaySettings {
            public int Width;
            public int Height;
            public int BitDepth;
            public int RefreshRate;
        }

        private static bool HasDisplaySetting(IEnumerable<DisplaySettings> availableDisplaySettings,
            DisplaySettings check) {
            return availableDisplaySettings.Any(item => item.Width == check.Width
                                                        && item.BitDepth == check.BitDepth &&
                                                        item.Height == check.Height
                                                        && item.RefreshRate == check.RefreshRate);
        }

        public FREObject GetDisplayDevices(FREContext ctx, uint argc, FREObject[] argv) {
            var tmp = new FREObject().Init("Vector.<com.tuarua.DisplayDevice>", null);
            var vecDisplayDevices = new FREArray(tmp);

            var dd = new DisplayDevice();
            dd.cb = Marshal.SizeOf(dd);

            _displayDeviceMap.Clear();

            try {
                uint index = 0;
                uint cnt = 0;
                while (WinApi.EnumDisplayDevices(null, index++, ref dd, 0)) {
                    var displayDevice = new FREObject().Init("com.tuarua.DisplayDevice", null);
                    var displayMonitor = new FREObject().Init("com.tuarua.Monitor", null);

                    displayDevice.SetProp("isPrimary",
                        dd.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice));
                    displayDevice.SetProp("isActive",
                        dd.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop));
                    displayDevice.SetProp("isRemovable", dd.StateFlags.HasFlag(DisplayDeviceStateFlags.Removable));
                    displayDevice.SetProp("isVgaCampatible",
                        dd.StateFlags.HasFlag(DisplayDeviceStateFlags.VgaCompatible));

                    var monitor = new DisplayDevice();
                    monitor.cb = Marshal.SizeOf(monitor);

                    if (!WinApi.EnumDisplayDevices(dd.DeviceName, index - 1, ref monitor, 0)) {
                        continue;
                    }

                    var dm = new Devmode();
                    dm.dmSize = (short) Marshal.SizeOf(dm);
                    if (WinApi.EnumDisplaySettings(dd.DeviceName, WinApi.EnumCurrentSettings, ref dm) == 0) {
                        continue;
                    }

                    var availdm = new Devmode();
                    availdm.dmSize = (short) Marshal.SizeOf(availdm);
                    IList<DisplaySettings> availableDisplaySettings = new List<DisplaySettings>();

                    var freAvailableDisplaySettings = new FREArray(displayDevice.GetProp("availableDisplaySettings"));

                    uint cntAvailableSettings = 0;
                    for (var iModeNum = 0;
                        WinApi.EnumDisplaySettings(dd.DeviceName, iModeNum, ref availdm) != 0;
                        iModeNum++) {
                        var settings = new DisplaySettings {
                            Width = availdm.dmPelsWidth,
                            Height = availdm.dmPelsHeight,
                            BitDepth = Convert.ToInt32(availdm.dmBitsPerPel),
                            RefreshRate = availdm.dmDisplayFrequency
                        };

                        if (HasDisplaySetting(availableDisplaySettings, settings)) continue;
                        availableDisplaySettings.Add(settings);

                        var displaySettings = new FREObject().Init("com.tuarua.DisplaySettings", null);

                        displaySettings.SetProp("width", availdm.dmPelsWidth);
                        displaySettings.SetProp("height", availdm.dmPelsHeight);
                        displaySettings.SetProp("refreshRate", availdm.dmDisplayFrequency);
                        displaySettings.SetProp("bitDepth", Convert.ToInt32(availdm.dmBitsPerPel));
                        freAvailableDisplaySettings.Set(cntAvailableSettings, displaySettings);
                        cntAvailableSettings++;
                    }

                    displayMonitor.SetProp("friendlyName", monitor.DeviceString);
                    displayMonitor.SetProp("name", monitor.DeviceName);
                    displayMonitor.SetProp("id", monitor.DeviceID);
                    displayMonitor.SetProp("key", monitor.DeviceKey);

                    displayDevice.SetProp("friendlyName", dd.DeviceString);
                    displayDevice.SetProp("name", dd.DeviceName);
                    displayDevice.SetProp("id", dd.DeviceID);
                    displayDevice.SetProp("key", dd.DeviceKey);

                    var currentDisplaySettings = new FREObject().Init("com.tuarua.DisplaySettings", null);

                    currentDisplaySettings.SetProp("width", dm.dmPelsWidth);
                    currentDisplaySettings.SetProp("height", dm.dmPelsHeight);
                    currentDisplaySettings.SetProp("refreshRate", dm.dmDisplayFrequency);
                    currentDisplaySettings.SetProp("bitDepth", Convert.ToInt32(dm.dmBitsPerPel));

                    displayDevice.SetProp("currentDisplaySettings", currentDisplaySettings);
                    displayDevice.SetProp("monitor", displayMonitor);

                    vecDisplayDevices.Set(cnt, displayDevice);

                    _displayDeviceMap.Add(dd.DeviceKey, dd);

                    cnt++;
                }
            }
            catch (Exception e) {
                Trace(e.Message);
            }

            return vecDisplayDevices.RawValue;
        }

        public FREObject SetDisplayResolution(FREContext ctx, uint argc, FREObject[] argv) {
            var key = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            var newWidth = Convert.ToInt32(new FreObjectSharp(argv[1]).Value);
            var newHeight = Convert.ToInt32(new FreObjectSharp(argv[2]).Value);
            var newRefreshRate = Convert.ToInt32(new FreObjectSharp(argv[3]).Value);

            if (!string.IsNullOrEmpty(key)) {
                var device = _displayDeviceMap[key];
                var dm = new Devmode();
                dm.dmSize = (short) Marshal.SizeOf(dm);

                if (WinApi.EnumDisplaySettings(device.DeviceName, WinApi.EnumCurrentSettings, ref dm) == 0) {
                    return new FreObjectSharp(false).RawValue;
                }

                dm.dmPelsWidth = newWidth;
                dm.dmPelsHeight = newHeight;

                var flgs = DevModeFlags.DM_PELSWIDTH | DevModeFlags.DM_PELSHEIGHT;

                if (newRefreshRate > 0) {
                    flgs |= DevModeFlags.DM_DISPLAYFREQUENCY;
                    dm.dmDisplayFrequency = newRefreshRate;
                }

                dm.dmFields = (int) flgs;

                return WinApi.ChangeDisplaySettings(ref dm, (int) ChangeDisplaySettingsFlags.CdsTest) != 0
                    ? new FreObjectSharp(false).RawValue
                    : new FreObjectSharp(WinApi.ChangeDisplaySettings(ref dm, 0) == 0).RawValue;
            }
            return FREObject.Zero;
        }

        public FREObject RestartApp(FREContext ctx, uint argc, FREObject[] argv) {
            var delay = Convert.ToInt32(new FreObjectSharp(argv[0]).Value);
            var wmiQuery =
                $"select CommandLine from Win32_Process where Name='{Process.GetCurrentProcess().ProcessName}.exe'";
            var searcher = new ManagementObjectSearcher(wmiQuery);
            var retObjectCollection = searcher.Get();
            var sf = (from ManagementObject retObject in retObjectCollection select $"{retObject["CommandLine"]}")
                .FirstOrDefault();
            if (string.IsNullOrEmpty(sf)) return new FreObjectSharp(false).RawValue;
            var info = new ProcessStartInfo {
                Arguments = "/C ping 127.0.0.1 -n " + delay + " && " + sf,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(info);
            return new FreObjectSharp(true).RawValue;
        }
		
		
		/* DEDOSMEDIA FEATURES*/
		
		public FREObject FindTaskBar(FREContext ctx, uint argc, FREObject[] argv)
        {
            
            Hwnd handler = WinApi.FindWindow("Shell_TrayWnd", null);
            if( handler == IntPtr.Zero)
                return new FreObjectSharp("FindTaskBar:: IS NOT FOUND").RawValue;

            int dwExtStyle = WinApi.GetWindowLong(handler, -20);
            dwExtStyle &= ~0x00000008;

            WinApi.SetWindowLong(handler, -20, dwExtStyle);

            _foundWindow = handler;
            return new FreObjectSharp("FindTaskBar:: FOUND!!!!!!!").RawValue;
            
            
        }

        public FREObject IsProgramRunning(FREContext ctx, uint argc, FREObject[] argv)
        {
            var programPath = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            foreach (var pList in Process.GetProcessesByName(programPath))
            {                
                return new FreObjectSharp(true).RawValue;
            }
            return new FreObjectSharp(false).RawValue;
        }

        public FREObject Test(FREContext ctx, uint argc, FREObject[] argv)
        {

            var test = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            string str = "Testing S3 client";
            /*
            Console.WriteLine("Listing objects stored in a bucket");

            using (client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
            {
                str += "Listing objects stored in a bucket";
                ListingObjects();
                str += "despues de async. \n";
                //SendEvent("MY_EVENT", "this is a test");
                //Trace("HOLA MUNDO");
            }
            */
            Trace("Prueba de Test");
            return str.ToFREObject();
            //return new FreObjectSharp(str).RawValue;

        }

        public FREObject ReadIniValue(FREContext ctx, uint argc, FREObject[] argv)
        {
            var section = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            var key = Convert.ToString(new FreObjectSharp(argv[1]).Value);
            var filepath = Convert.ToString(new FreObjectSharp(argv[2]).Value);
            string str = ReadValue(section, key, filepath, "");
            return new FreObjectSharp(str).RawValue;
        }

        public string ReadValue(string section, string key, string filePath, string defaultValue = "")
        {
            var value = new StringBuilder(512);
            WinApi.GetPrivateProfileString(section, key, defaultValue, value, value.Capacity, filePath);
            return value.ToString();
        }

        /*
        async void ListingObjects()
        {
            Trace("listingObject");
            string data = await ListingObjectsAsync();
            Trace(data);
        }

        // List S3 bucket objects
        
        async Task<string> ListingObjectsAsync()
        {
           
            string str = "";
            try
            {
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = "keshot-dedosmedia",
                    MaxKeys = 2
                };
                do
                {
                    Task<ListObjectsResponse> answer =  client.ListObjectsAsync(request);
                    // Process response.

                    ListObjectsResponse response = await answer;
                    foreach (S3Object entry in response.S3Objects)
                    {
                        str += "key = " + entry.Key + " size = " + entry.Size+"\n";       
                    }

                    

                    // If response is truncated, set the marker to get the next 
                    // set of keys.
                    if (response.IsTruncated)
                    {
                        request.Marker = response.NextMarker;
                    }
                    else
                    {
                        request = null;
                    }
                } while (request != null);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                    "To sign up for service, go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                     "Error occurred. Message:'{0}' when listing objects",
                     amazonS3Exception.Message);
                }
            }
            return str;
        }
        */
        

        public FREObject TestCV(FREContext ctx, uint argc, FREObject[] argv)
        {
            /*
            String win1 = "Test Window"; //The name of the window
            
            CvInvoke.NamedWindow(win1); //Create the window using the specific name

            Mat img = new Mat(200, 400, DepthType.Cv8U, 3); //Create a 3 channel image of 400x200
            img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

            //Draw "Hello, world." on the image using the specific font
            CvInvoke.PutText(
               img,
               "Hello, world",
               new System.Drawing.Point(10, 80),
               FontFace.HersheyComplex,
               1.0,
               new Bgr(0, 255, 0).MCvScalar);


            CvInvoke.Imshow(win1, img); //Show the image
            CvInvoke.WaitKey(0);  //Wait for the key pressing event
            CvInvoke.DestroyWindow(win1); //Destroy the window if key is pressed
            */
            return new FreObjectSharp(true).RawValue;
        }

        public FREObject UnzipFile(FREContext ctx, uint argc, FREObject[] argv)
        {
            var zipFile = Convert.ToString(new FreObjectSharp(argv[0]).Value);
            var outputDirectory = Convert.ToString(new FreObjectSharp(argv[1]).Value);
            var password = Convert.ToString(new FreObjectSharp(argv[2]).Value);
            
            if (!ZipFile.CheckZipPassword(zipFile, password))
            {
                return new FreObjectSharp(false).RawValue;
            }
            ZipFile zip = ZipFile.Read(zipFile);
            zip.Password = password;
            Directory.CreateDirectory(outputDirectory);
            zip.ExtractAll(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();
            
            return new FreObjectSharp(true).RawValue;
        }

        public FREObject MakeTopMostWindow(FREContext ctx, uint argc, FREObject[] argv)
        {
            var value = WinApi.SetWindowPos(_foundWindow, new IntPtr(-1), 0, 0, 0, 0, WindowPositionFlags.SWP_NOSIZE | WindowPositionFlags.SWP_NOMOVE);
            return new FreObjectSharp(value).RawValue;
        }

        public FREObject MakeBottomWindow(FREContext ctx, uint argc, FREObject[] argv)
        {
            var value = WinApi.SetWindowPos(_foundWindow, new IntPtr(1), 0, 0, 0, 0, WindowPositionFlags.SWP_NOSIZE | WindowPositionFlags.SWP_NOMOVE | WindowPositionFlags.SWP_NOZORDER | WindowPositionFlags.SWP_FRAMECHANGED);
            return new FreObjectSharp(value).RawValue;
        }

        public FREObject MakeNoTopMostWindow(FREContext ctx, uint argc, FREObject[] argv)
        {
            var value = WinApi.SetWindowPos(_foundWindow, new IntPtr(-2), 0, 0, 0, 0, WindowPositionFlags.SWP_NOSIZE | WindowPositionFlags.SWP_NOMOVE);
            return new FreObjectSharp(value).RawValue;
        }

        public FREObject ResizeWindow(FREContext ctx, uint argc, FREObject[] argv) {            
            var newX = Convert.ToInt32(new FreObjectSharp(argv[0]).Value);
            var newY = Convert.ToInt32(new FreObjectSharp(argv[1]).Value);
            var newW = Convert.ToInt32(new FreObjectSharp(argv[2]).Value);
            var newH = Convert.ToInt32(new FreObjectSharp(argv[3]).Value);
            var value = WinApi.SetWindowPos(_foundWindow, new IntPtr(0), newX, newY, newW, newH, WindowPositionFlags.SWP_NOZORDER);
            return new FreObjectSharp(value).RawValue;
        }
    }
}