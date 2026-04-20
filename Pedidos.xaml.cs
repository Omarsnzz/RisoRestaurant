using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class Pedidos : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=risorestaurant;";
        decimal totalAcumulado = 0;
        List<string> productosEnCarrito = new List<string>();

        public Pedidos()
        {
            InitializeComponent();
            CargarDatos();    // Carga la tabla de pedidos
            CargarMenus();    // Llena los ComboBox de platillos y bebidas
        }

        // --- CARGA DE DATOS ---
        private void CargarMenus()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    // Cargar Platillos (Asegúrate que la tabla se llame 'alimentos')
                    MySqlDataAdapter daAlimentos = new MySqlDataAdapter("SELECT Nombre, Precio FROM alimentos", conexion);
                    DataTable dtAlimentos = new DataTable();
                    daAlimentos.Fill(dtAlimentos);
                    cmbPlatillos.ItemsSource = dtAlimentos.DefaultView;

                    // Cargar Bebidas
                    MySqlDataAdapter daBebidas = new MySqlDataAdapter("SELECT Nombre, Costo FROM bebidas", conexion);
                    DataTable dtBebidas = new DataTable();
                    daBebidas.Fill(dtBebidas);
                    cmbBebidas.ItemsSource = dtBebidas.DefaultView;
                }
                catch (Exception ex) { MessageBox.Show("Error al cargar menús: " + ex.Message); }
            }
        }

        private void CargarDatos()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT * FROM pedidos_domicilio";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgPedidos.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        // --- LÓGICA DEL CARRITO Y PRECIOS ---
        private void btnAgregarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlatillos.SelectedItem != null)
            {
                DataRowView row = (DataRowView)cmbPlatillos.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";
                decimal precio = Convert.ToDecimal(row["Precio"]);

                AgregarAlCarrito(nombre, precio);
            }
        }

        private void btnAgregarBebida_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBebidas.SelectedItem != null)
            {
                DataRowView row = (DataRowView)cmbBebidas.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";
                decimal precio = Convert.ToDecimal(row["Costo"]);

                AgregarAlCarrito(nombre, precio);
            }
        }

        private void AgregarAlCarrito(string nombre, decimal precio)
        {
            productosEnCarrito.Add(nombre);
            lbCarrito.Items.Add($"{nombre} - ${precio}");
            totalAcumulado += precio;
            lblTotal.Text = totalAcumulado.ToString("C"); // Formato moneda
        }

        

        // --- GUARDADO EN BASE DE DATOS ---
        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (productosEnCarrito.Count == 0 || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Faltan datos o el carrito está vacío.");
                return;
            }

            // Unimos todos los platillos en un solo texto separado por comas
            string listaComida = string.Join(", ", productosEnCarrito);

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "INSERT INTO pedidos_domicilio (Nombre, Telefono, Lugar, Comida, Dia, Hora, Total, Estado) " +
                                   "VALUES (@nom, @tel, @lug, @com, @dia, @hor, @tot, @est)";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nom", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@tel", txtTelefono.Text);
                    cmd.Parameters.AddWithValue("@lug", txtLugar.Text);
                    cmd.Parameters.AddWithValue("@com", listaComida);
                    cmd.Parameters.AddWithValue("@dia", dpDia.SelectedDate);
                    cmd.Parameters.AddWithValue("@hor", txtHora.Text);
                    cmd.Parameters.AddWithValue("@tot", totalAcumulado);
                    cmd.Parameters.AddWithValue("@est", ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("¡Pedido guardado!");
                    LimpiarFormulario();
                    CargarDatos();
                }
                catch (Exception ex) { MessageBox.Show("Error al guardar: " + ex.Message); }
            }
        }

        // --- OTROS BOTONES ---
        private void dgPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPedidos.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgPedidos.SelectedItem;
                txtFolio.Text = row["Folio"].ToString();
                txtNombre.Text = row["Nombre"].ToString();
                // Al editar, el sistema cargará los datos antiguos, pero el total se recalcula si agregas más cosas.
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolio.Text)) return;
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "DELETE FROM pedidos_domicilio WHERE Folio=@folio";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@folio", txtFolio.Text);
                cmd.ExecuteNonQuery();
                CargarDatos();
                LimpiarFormulario();
            }
        }

        private void LimpiarCarrito()
        {
            productosEnCarrito.Clear();
            lbCarrito.Items.Clear();
            totalAcumulado = 0;
            lblTotal.Text = "$0.00";
        }

        private void btnLimpiarCarrito_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCarrito();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtTelefono.Clear();
            txtLugar.Clear();
            txtHora.Clear();
            LimpiarCarrito();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow principal = new MainWindow();
            principal.Show();
            this.Close();
        }

        // Método vacío para cumplir con el XAML si no lo usas
        private void btnEditar_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Función de edición rápida: Agregue productos y pulse Guardar como nuevo o use Eliminar."); }
    }
}