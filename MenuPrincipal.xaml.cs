using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class MainWindow : Window
    {
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
                    MessageBox.Show("Módulo: Platillos");
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
                    Bebidas ventanaBebidas = new Bebidas();
                    ventanaBebidas.Show();
                    this.Close();
                    break;
            }
        }
    }
}