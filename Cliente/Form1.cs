using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace Cliente
{
    public partial class Form1 : Form
    {
        

        private const string SeleccioneFolder = "Seleccione una ruta de trabajo valida";

        private const string BrowserDescrip = "Selecciona la carpeta de trabajo";

        private IPAddress ip;

        private string archivo = "";

        private long tamanio = 0;

        private static int totalRead = 0;

        private static bool descargando = false;

        private static List<ServicioCentral.Archivo> l_source = new List<ServicioCentral.Archivo>();

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 5000;
            timer1.Enabled = true;
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = BrowserDescrip;
            DialogResult res;
            do
            {
                res = folder.ShowDialog();
                Program.RutaDeTrabajo = folder.SelectedPath;
                if(res != DialogResult.OK)
                {
                    MessageBox.Show(SeleccioneFolder);
                }
            } while(res != DialogResult.OK);
            EnviarInfoArchivos();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            var lista = Program.serv.GetLista();
            var source = new List<ServicioCentral.Archivo>(lista).Where(x => x.ipFuente != Program.IP_Local).ToList();
            //source = source.Where
            l_source = source;
            dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
            dataGridView1.DataSource = source;
            dataGridView1.Refresh();
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            
        }

        private void EnviarInfoArchivos()
        {
            var a = Directory.GetFiles(Program.RutaDeTrabajo);
            foreach (var item in a)
            {
                var info = new FileInfo(item);
                
                Program.serv.AddArchivo(info.Name,Program.IP_Local, info.Length);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.serv.CerrarPeer(Program.IP_Local);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows != null && dataGridView1.SelectedRows.Count>0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                if (!IPAddress.TryParse(row.Cells[2].Value.ToString(), out ip))
                {
                    MessageBox.Show("Dirección IP Invalida");
                    return;
                }
                archivo = row.Cells[1].Value.ToString();
                tamanio = Convert.ToInt64(row.Cells[0].Value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Thread t = new Thread(Recibir);
            t.Start();
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Continuous;
            Thread.Sleep(100);
            while (descargando)
            {
                progressBar1.Value = (int)((100d * totalRead) / tamanio);
                //MessageBox.Show(((int)((100d * totalRead) / tamanio)).ToString());
                //Thread.Sleep(50);
            }
            Thread.Sleep(500);
            progressBar1.Value = 0;
            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.PermitirDescargas = checkBox1.Checked;
        }

        public async void Recibir()
        {
            if (ip != null)
            {
                TcpClient client = new TcpClient();
                try
                {
                    client.Connect(ip, Program.PORT);
                    descargando = true;
                }
                catch
                {
                    MessageBox.Show("El cliente " + ip + " no permite descargas");
                    return;
                }
                NetworkStream ns = client.GetStream();

                {
                    byte[] fileName = ASCIIEncoding.ASCII.GetBytes(archivo);
                    byte[] fileNameLength = BitConverter.GetBytes(archivo.Length);
                    byte[] fileLength = BitConverter.GetBytes(tamanio);

                    await ns.WriteAsync(fileLength, 0, fileLength.Length);
                    await ns.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                    await ns.WriteAsync(fileName, 0, fileName.Length);
                }
                if(!Directory.Exists(Program.RutaDeTrabajo + "/Descargas"))
                {
                    Directory.CreateDirectory(Program.RutaDeTrabajo + "/Descargas");
                }
                if(File.Exists(Program.RutaDeTrabajo + "/Descargas/" + archivo))
                {
                    File.Delete(Program.RutaDeTrabajo + "/Descargas/" + archivo);
                }
                FileStream fileStream = File.Open(Program.RutaDeTrabajo +"/Descargas/" +archivo, FileMode.Create);

                // Recibir
                
                int read;
                
                byte[] buffer = new byte[32 * 1024];
                while ((read = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;
                }
                totalRead = 0;
                descargando = false;
                MessageBox.Show("Recibido", "Aviso",MessageBoxButtons.OK,MessageBoxIcon.Information);
                fileStream.Dispose();
                client.Close();
            }
            else
            {
                MessageBox.Show("No se selecciono el archivo");
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var lista = Program.serv.GetLista();
            var source = lista.ToList();
                //new List<ServicioCentral.Archivo>(lista).Where(x => x.ipFuente != Program.IP_Local).ToList();
            int count = 0;
            if (source.Count != l_source.Count)
            {
                foreach (var item in source)
                {
                    foreach (var item2 in l_source)
                    {
                        if (item != item2)
                        {
                            count++;
                        }
                    }

                }
            }
            if (count != 0 || source.Count != l_source.Count)
            {
                dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
                dataGridView1.DataSource = source;
                l_source = source;
                dataGridView1.Refresh();
                dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            }
        }
    }
}
