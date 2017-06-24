using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliente
{
    static class Program
    {
        public static ServicioCentral.ServiceClient serv;

        public static Thread hiloServir = new Thread(Peticiones.SirvePeticiones);

        public static bool PermitirDescargas = false;

        public static int PORT = 1723;

        public static string RutaDeTrabajo = "";

        public static string IP_Local = "";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP_Local = ip.ToString();
                }
            }
            serv = new ServicioCentral.ServiceClient();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            hiloServir.Start();
            Application.Run(new Form1());
            hiloServir.Abort();
        }
    }
}
