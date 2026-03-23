using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class EliminarBebida : Window
    {
        string cadenaConexion = "Server=localhost; Database=RisoRestaurant; Uid=root; Pwd=;";

        public EliminarBebida()
        {
            InitializeComponent();
            CargarBebidas(); // Carga la tabla al abrir la ventana
        }

        private void CargarBebidas()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string consulta = "SELECT idBebidas, Nombre, Costo FROM Bebidas";
                MySqlCommand comando = new MySqlCommand(consulta, conexion);
                MySqlDataAdapter adaptador = new MySqlDataAdapter(comando);
                DataTable tablaBebidas = new DataTable();
                adaptador.Fill(tablaBebidas);
                dgBebidas.ItemsSource = tablaBebidas.DefaultView;
            }
        }

        private void btnBorrar_Click(object sender, RoutedEventArgs e)
        {
            if (dgBebidas.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una bebida de la tabla primero.");
                return;
            }

            DataRowView filaSeleccionada = (DataRowView)dgBebidas.SelectedItem;
            int idBebida = Convert.ToInt32(filaSeleccionada["idBebidas"]);

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string consulta = "DELETE FROM Bebidas WHERE idBebidas = @id";
                MySqlCommand comando = new MySqlCommand(consulta, conexion);
                comando.Parameters.AddWithValue("@id", idBebida);
                comando.ExecuteNonQuery();

                MessageBox.Show("Bebida eliminada.");
                CargarBebidas(); // Refresca la tabla
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
