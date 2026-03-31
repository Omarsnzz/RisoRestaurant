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
                    MessageBox.Show("Módulo: Inventario");
                    break;

                case "btnPedidos":
                    MessageBox.Show("Módulo: Pedidos");
                    break;

                case "btnCuenta":
                    MessageBox.Show("Módulo: Cuentas");
                    break;

                case "btnBebidas":
                    // 1. Instanciamos la ventana de Bebidas
                    Bebidas ventanaBebidas = new Bebidas();

                    // 2. Mostramos la nueva ventana
                    ventanaBebidas.Show();

                    // 3. Ocultamos el Menú Principal (usar Hide en lugar de Close)
                    this.Hide();
                    break;
            }
        }
    }
}