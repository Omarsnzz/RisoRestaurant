using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class MainWindow : Window
    {
        // Cadena de conexión a tu base de datos
        string cadenaConexion = "server=localhost;port=3306;user=root;password=admin123;database=RisoRestaurant;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Button boton = (Button)sender;

            switch (boton.Name)
            {
                case "btnPlatillos":
                    PlatillosDia ventanaPlatillos = new PlatillosDia();
                    ventanaPlatillos.Show();
                    this.Hide();
                    break;

                case "btnInventario":
                    VentanaInventario ventanaInventario = new VentanaInventario();
                    ventanaInventario.Show();
                    this.Hide();

                    break;

                case "btnPedidos":
                    MessageBox.Show("Módulo: Pedidos");
                    break;

                case "btnCuenta":
                    Cuenta ventanaCuenta = new Cuenta();
                    ventanaCuenta.Show();
                    this.Hide();
                    break;

                case "btnBebidas":
                    VentanaBebidas ventanaBebidas = new VentanaBebidas();
                    ventanaBebidas.Show();
                    this.Hide();

                    break; 

            }
        }
    }
}