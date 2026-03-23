using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Riso
{
    public partial class AgregarBebida : Window
    {
        string cadenaConexion = "Server=localhost; Database=RisoRestaurant; Uid=root; Pwd=;";

        public AgregarBebida()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    conexion.Open();
                    string consulta = "INSERT INTO Bebidas (Nombre, Costo) VALUES (@nombre, @costo)";
                    MySqlCommand comando = new MySqlCommand(consulta, conexion);
                    comando.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    comando.Parameters.AddWithValue("@costo", txtPrecio.Text);
                    comando.ExecuteNonQuery();

                    MessageBox.Show("¡Bebida agregada con éxito!");
                    txtNombre.Clear();
                    txtPrecio.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            Bebidas ventanaBebidas = new Bebidas();
            ventanaBebidas.Show();
            this.Close();
        }
    }
}
