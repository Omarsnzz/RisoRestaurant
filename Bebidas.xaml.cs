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
    public partial class Bebidas : Window
    {
        // La ruta para llegar a tu base de datos de XAMPP
        string cadenaConexion = "Server=localhost; Database=RisoRestaurant; Uid=root; Pwd=;";

        public Bebidas()
        {
            InitializeComponent();
            CargarDatosTabla(); // Esto llena la tabla en cuanto se abre la ventana
        }

        // Esta es la función que te marcaba error porque no existía
        private void CargarDatosTabla()
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    conexion.Open();
                    // Seleccionamos las columnas exactas de tu tabla Bebidas
                    string consulta = "SELECT idBebidas, Nombre, Costo FROM Bebidas";
                    MySqlCommand comando = new MySqlCommand(consulta, conexion);
                    MySqlDataAdapter adaptador = new MySqlDataAdapter(comando);

                    DataTable tabla = new DataTable();
                    adaptador.Fill(tabla);

                    // Asigna los datos al DataGrid de tu diseño
                    dgBebidas.ItemsSource = tabla.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos: " + ex.Message);
            }
        }

        private void btnAgregarBebida_Click(object sender, RoutedEventArgs e)
        {
            AgregarBebida ventana = new AgregarBebida();
            ventana.Show();
            this.Close();
        }

        private void btnEliminarBebida_Click(object sender, RoutedEventArgs e)
        {
            EliminarBebida ventana = new EliminarBebida();
            ventana.Show();
            this.Close();
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatosTabla(); // Ahora sí existe y funcionará el botón
            MessageBox.Show("Tabla actualizada");
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            // OJO: Si tu ventana principal se llama MenuPrincipal, cámbialo aquí
            MainWindow menu = new MainWindow();
            menu.Show();
            this.Close();
        }
    }
}
