using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cliente
{
    public class Peticiones
    {
        public static async void SirvePeticiones()
        {
            while(true)
            {
                try
                {
                    TcpListener listener = TcpListener.Create(Program.PORT);
                    if (Program.PermitirDescargas)
                    {
                        listener.Start();
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        NetworkStream ns = client.GetStream();
                    
                        string fileName;
                        {
                            byte[] fileNameBytes;
                            byte[] fileNameLengthBytes = new byte[4]; //int32
                            byte[] fileLengthBytes = new byte[8]; //int64

                            await ns.ReadAsync(fileLengthBytes, 0, 8); // int64
                            await ns.ReadAsync(fileNameLengthBytes, 0, 4); // int32
                            fileNameBytes = new byte[BitConverter.ToInt32(fileNameLengthBytes, 0)];
                            await ns.ReadAsync(fileNameBytes, 0, fileNameBytes.Length);

                            fileName = ASCIIEncoding.ASCII.GetString(fileNameBytes);
                        }

                        var info = new FileInfo(Program.RutaDeTrabajo + "\\" + fileName);
                        var file = info.OpenRead();
                        int read;
                        int totalWritten = 0;
                        byte[] buffer = new byte[32 * 1024]; // 32k chunks
                        while ((read = await file.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await ns.WriteAsync(buffer, 0, read);
                            totalWritten += read;
                        }
                        file.Dispose();
                        client.Close();
                    }
                }
                catch(Exception e)
                {
                    
                }
                
            }
        }
    }
}
