using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class MainWindow : Window
    {
        // Tu cadena de conexión verificada
        string cadenaConexion = "server=localhost;port=3306;user=root;password=admin123;database=RisoRestaurant;";

        public MainWindow()
        {
            InitializeComponent();
        }

        // Lógica para los botones del menú
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Button boton = (Button)sender;

            // Aquí definiremos qué ventana abrir dependiendo del botón
            switch (boton.Name)
            {
                case "btnPlatillos":
                    MessageBox.Show("Módulo de Platillos en construcción...");
                    break;
                case "btnInventario":
                    MessageBox.Show("Módulo de Inventario en construcción...");
                    break;
                case "btnPedidos":
                    // Aquí es donde llamaremos a la ventana de pedidos que hicimos antes
                    MessageBox.Show("Módulo de Pedidos a Domicilio seleccionado.");
                    break;
                case "btnCuenta":
                    MessageBox.Show("Módulo de Cuentas seleccionado.");
                    break;
                case "btnBebidas":
                    // Abrimos la nueva ventana de Bebidas
                    Bebidas ventanaBebidas = new Bebidas();
                    ventanaBebidas.Show();

                    // Cerramos el menú principal
                    this.Close();
                    break;
            }
        }

        // Tu método original de prueba
        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection conexionBD = new MySqlConnection(cadenaConexion);

            try
            {
                conexionBD.Open();
                MessageBox.Show("¡Conexión Exitosa con RisoRestaurant!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                conexionBD.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error al conectar: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}