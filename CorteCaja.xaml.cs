using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class CorteCaja : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=RisoRestaurant;";

        public CorteCaja()
        {
            InitializeComponent();
            dpFecha.SelectedDate = DateTime.Today;
        }

        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFecha.SelectedDate.HasValue)
            {
                CargarCorte(dpFecha.SelectedDate.Value);
            }
        }

        private void CargarCorte(DateTime fechaBusqueda)
        {
            string fechaSql = fechaBusqueda.ToString("yyyy-MM-dd");
            lblTituloFecha.Text = "Corte del día: " + fechaBusqueda.ToString("dd/MM/yyyy");

            double totalRestaurante = 0;
            double totalDomicilio = 0;

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    conexion.Open();

                    string queryRes = "SELECT SUM(Total) FROM cuenta WHERE DATE(FechaHora) = @fecha";
                    MySqlCommand cmd1 = new MySqlCommand(queryRes, conexion);
                    cmd1.Parameters.AddWithValue("@fecha", fechaSql);
                    object res1 = cmd1.ExecuteScalar();
                    totalRestaurante = res1 != DBNull.Value ? Convert.ToDouble(res1) : 0;

                    string queryDom = "SELECT SUM(Total) FROM pedidos_domicilio WHERE DATE(Dia) = @fecha";
                    MySqlCommand cmd2 = new MySqlCommand(queryDom, conexion);
                    cmd2.Parameters.AddWithValue("@fecha", fechaSql);
                    object res2 = cmd2.ExecuteScalar();
                    totalDomicilio = res2 != DBNull.Value ? Convert.ToDouble(res2) : 0;
                }

                txtVentasRestaurante.Text = totalRestaurante.ToString("C");
                txtVentasDomicilio.Text = totalDomicilio.ToString("C");
                txtTotalGeneral.Text = (totalRestaurante + totalDomicilio).ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar: " + ex.Message);
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow principal = new MainWindow();
            principal.Show();
            this.Close();
        }
    }
}