using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using MySql.Data.MySqlClient; //Referencia para trabajar en lOCALHOST en Xamp
using System.Text.RegularExpressions;

namespace _30LIDS_ROJO_EXAMENP2
{
    public partial class Form1 : Form
    {
        delegate void SetTextDelegate(string value);
        public SerialPort ArduinoPort
        {
            get;
        }
        //Datos de conexion a MySql (Xamp)
        string conexionSql = "Server=localhost;Port=3306;Database=examenp2;Uid=root;Pwd=;";
        public Form1()
        {
            InitializeComponent();
            ArduinoPort = new System.IO.Ports.SerialPort();
            ArduinoPort.PortName = "COM8";
            ArduinoPort.BaudRate = 9600;
            ArduinoPort.DataBits = 8;
            ArduinoPort.ReadTimeout = 500;
            ArduinoPort.WriteTimeout = 500;
            ArduinoPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            this.btnConectar.Click += btnConectar_Click;
            ArduinoPort.Open();
            //Led Codigo 
            //vincular Eventos
            this.FormClosing += CerrandoForm1;
            this.btnApagar.Click += btnApagar_Click;
            this.btnEncender.Click += btnEncender_Click;

            /*
            txtNombre.TextChanged += ValidarNombre;
            txtApellido.TextChanged += ValidarApellidos;*/
        }
        private void DataReceivedHandler(object sneder, SerialDataReceivedEventArgs e)
        {
            string dato = ArduinoPort.ReadLine();
            EscribirTxt(dato);
        }

        private void EscribirTxt(string dato)
        {
            if (InvokeRequired)
                try
                {
                    Invoke(new SetTextDelegate(EscribirTxt), dato);
                }
                catch { }
            else
                lblTemperatura.Text = dato;
        }

        //Insertar Y validar Registros de nombre
        private void InsertarRegistro(string nombre, string apellidos)
        {
            using (MySqlConnection conection = new MySqlConnection(conexionSql))
            {
                conection.Open();

                string insertQuery = "INSERT INTO registros (Nombre, Apellidos)" + "VALUES (@Nombre, @Apellidos)";                

                using (MySqlCommand command = new MySqlCommand(insertQuery, conection))
                {
                    command.Parameters.AddWithValue("@Nombre", nombre);
                    command.Parameters.AddWithValue("@Apellidos", apellidos);                    
                    command.ExecuteNonQuery();
                }
                conection.Close();
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            btnDesconectar.Enabled = true;
            try
            {
                if (!ArduinoPort.IsOpen)
                    ArduinoPort.Open();
                if (int.TryParse(txtLimTem.Text, out int temperatureLimit))
                {
                    //Convierte
                    string limitString = temperatureLimit.ToString();
                    ArduinoPort.Write(limitString);
                }
                else
                {
                    MessageBox.Show("Ingresa un valor númerico válido en el TextBox del limite de la Temperatura. ");
                }

                lblConetion.Text = "Conexión OK";
                lblConetion.ForeColor = System.Drawing.Color.Lime;
            }

            catch
            {
                MessageBox.Show("Configure el puerto de comunicacion correcto o Desconecte");
            }

        }

        private void btnDesconectar_Click(object sender, EventArgs e)
        {
            btnConectar.Enabled = true;
            btnDesconectar.Enabled = false;
            if (ArduinoPort.IsOpen)
                ArduinoPort.Close();
            lblConetion.Text = "Desconectado";
            lblConetion.ForeColor = System.Drawing.Color.Red;
            lblTemperatura.Text = "0.0";
        }

        private void btnEncender_Click(object sender, EventArgs e)
        {
            ArduinoPort.Write("b");
        }

        //Registo de Datos en MYSQL
        private void InsertarRegistro2(string encender)
        {
            using (MySqlConnection conection = new MySqlConnection(conexionSql))
            {
                conection.Open();

                string insertQuery = "INSERT INTO registros (Led)" + "VALUES (@Led)";

                using (MySqlCommand command = new MySqlCommand(insertQuery, conection))
                {
                    command.Parameters.AddWithValue("@Led", encender);                    
                    command.ExecuteNonQuery();
                }
                conection.Close();
            }
        }

        private void btnApagar_Click(object sender, EventArgs e)
        {
            ArduinoPort.Write("a");
            string encender = btnEncender.Text;
            InsertarRegistro2(encender);
        }
        private void CerrandoForm1(object sender, EventArgs e)
        {
            //Cerrando Puerto 
            if (ArduinoPort.IsOpen) ArduinoPort.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            //Obtener los datos en los TexBox 
            string nombres = txtNombre.Text;
            string apellidos = txtApellido.Text;
            InsertarRegistro(nombres, apellidos);            
            MessageBox.Show("Datos ingresados Correctamente");
        }
        
    }
}
