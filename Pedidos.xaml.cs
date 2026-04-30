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

        List<ItemDeduccion> itemsDeduccion = new List<ItemDeduccion>();

        private class ItemDeduccion
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public string TablaBD { get; set; } 
        }

        public Pedidos()
        {
            InitializeComponent();
            CargarDatos();
            CargarMenus();
        }

        private void CargarMenus()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    MySqlDataAdapter daAlimentos = new MySqlDataAdapter("SELECT Nombre, Precio FROM alimentos", conexion);
                    DataTable dtAlimentos = new DataTable();
                    daAlimentos.Fill(dtAlimentos);
                    cmbPlatillos.ItemsSource = dtAlimentos.DefaultView;

                    MySqlDataAdapter daBebidas = new MySqlDataAdapter("SELECT Nombre, Costo FROM bebidas", conexion);
                    DataTable dtBebidas = new DataTable();
                    daBebidas.Fill(dtBebidas);
                    cmbBebidas.ItemsSource = dtBebidas.DefaultView;
                }
                catch (Exception ex) { MessageBox.Show("Error al cargar menús: " + ex.Message); }
            }
        }

        // --- CARGA DE DATOS POR FECHA ---

        // Modificamos CargarDatos para que por defecto muestre SOLO los de HOY
        private void CargarDatos()
        {
            // Llama al nuevo método pasándole la fecha de hoy
            CargarDatosPorFecha(DateTime.Today);
        }

        // NUEVO MÉTODO: Trae solo los pedidos del día que le pidas
        private void CargarDatosPorFecha(DateTime fecha)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    // Usamos DATE(Dia) para asegurarnos de que compare solo la fecha y no la hora
                    string query = "SELECT * FROM pedidos_domicilio WHERE DATE(Dia) = @fecha";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);

                    // Le pasamos la fecha formateada para que MySQL la entienda (Año-Mes-Día)
                    cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgPedidos.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex) { MessageBox.Show("Error al filtrar por fecha: " + ex.Message); }
            }
        }

        // EVENTO: Botón para buscar una fecha específica
        private void btnBuscarFecha_Click(object sender, RoutedEventArgs e)
        {
            if (dpFiltroFecha.SelectedDate.HasValue)
            {
                // Si eligió una fecha, cargamos los datos de ese día
                CargarDatosPorFecha(dpFiltroFecha.SelectedDate.Value);
            }
            else
            {
                MessageBox.Show("Por favor, selecciona una fecha en el calendario para buscar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // EVENTO: Botón para regresar a ver solo los pedidos de hoy
        private void btnVerHoy_Click(object sender, RoutedEventArgs e)
        {
            dpFiltroFecha.SelectedDate = DateTime.Today; // Actualiza el calendario visual
            CargarDatosPorFecha(DateTime.Today); // Carga los datos
        }


        private int ObtenerStockDisponible(string nombreProducto, string tabla)
        {
            int stock = 0;
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = $"SELECT Cantidad FROM {tabla} WHERE Nombre = @nom";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nom", nombreProducto);

                    object resultado = cmd.ExecuteScalar();
                    if (resultado != null && resultado != DBNull.Value)
                    {
                        stock = Convert.ToInt32(resultado);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error al verificar inventario: " + ex.Message); }
            }
            return stock;
        }

        private void cmbPlatillos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPlatillos.SelectedItem != null)
            {
                DataRowView row = (DataRowView)cmbPlatillos.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";

                int stock = ObtenerStockDisponible(nombre, "alimentos");
                lblDisponiblePlatillo.Text = stock.ToString();
            }
        }

        private void cmbBebidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBebidas.SelectedItem != null)
            {
                DataRowView row = (DataRowView)cmbBebidas.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";

                int stock = ObtenerStockDisponible(nombre, "bebidas");
                lblDisponibleBebida.Text = stock.ToString();
            }
        }

        private void btnAgregarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlatillos.SelectedItem != null)
            {
                if (!int.TryParse(txtCantPlatillo.Text, out int cantidadPedida) || cantidadPedida <= 0)
                {
                    MessageBox.Show("Ingresa una cantidad válida para el platillo.", "Aviso");
                    return;
                }

                DataRowView row = (DataRowView)cmbPlatillos.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";
                decimal precio = Convert.ToDecimal(row["Precio"]);

                ProcesarIngresoAlCarrito(nombre, precio, cantidadPedida, "alimentos");
                txtCantPlatillo.Text = "1"; 
            }
        }

        private void btnAgregarBebida_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBebidas.SelectedItem != null)
            {
                if (!int.TryParse(txtCantBebida.Text, out int cantidadPedida) || cantidadPedida <= 0)
                {
                    MessageBox.Show("Ingresa una cantidad válida para la bebida.", "Aviso");
                    return;
                }

                DataRowView row = (DataRowView)cmbBebidas.SelectedItem;
                string nombre = row["Nombre"].ToString() ?? "";
                decimal precio = Convert.ToDecimal(row["Costo"]);

                ProcesarIngresoAlCarrito(nombre, precio, cantidadPedida, "bebidas");
                txtCantBebida.Text = "1"; 
            }
        }

        private void ProcesarIngresoAlCarrito(string nombre, decimal precioUnitario, int cantidadPedida, string tablaBD)
        {
            int stockTotal = ObtenerStockDisponible(nombre, tablaBD);


            int cantidadYaEnCarrito = itemsDeduccion.Where(x => x.Nombre == nombre).Sum(x => x.Cantidad);

            if (cantidadPedida + cantidadYaEnCarrito > stockTotal)
            {
                int disponibleParaAgregar = stockTotal - cantidadYaEnCarrito;
                MessageBox.Show($"No hay suficientes existencias de '{nombre}'.\n\nStock total: {stockTotal}\nYa en carrito: {cantidadYaEnCarrito}\nPuedes agregar máximo: {disponibleParaAgregar}", "Inventario Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal subtotal = precioUnitario * cantidadPedida;
            string descripcionItem = $"{cantidadPedida} x {nombre}";

            productosEnCarrito.Add(descripcionItem);
            lbCarrito.Items.Add($"{descripcionItem} - {subtotal:C}");

            totalAcumulado += subtotal;
            lblTotal.Text = totalAcumulado.ToString("C");

            itemsDeduccion.Add(new ItemDeduccion { Nombre = nombre, Cantidad = cantidadPedida, TablaBD = tablaBD });
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (productosEnCarrito.Count == 0 || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Faltan datos o el carrito está vacío.");
                return;
            }

            string listaComida = string.Join(", ", productosEnCarrito);

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();

                    string queryPedido = "INSERT INTO pedidos_domicilio (Nombre, Telefono, Lugar, Comida, Dia, Hora, Total, Estado) " +
                                   "VALUES (@nom, @tel, @lug, @com, @dia, @hor, @tot, @est)";
                    MySqlCommand cmd = new MySqlCommand(queryPedido, conexion);
                    cmd.Parameters.AddWithValue("@nom", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@tel", txtTelefono.Text);
                    cmd.Parameters.AddWithValue("@lug", txtLugar.Text);
                    cmd.Parameters.AddWithValue("@com", listaComida);
                    cmd.Parameters.AddWithValue("@dia", dpDia.SelectedDate);
                    cmd.Parameters.AddWithValue("@hor", txtHora.Text);
                    cmd.Parameters.AddWithValue("@tot", totalAcumulado);
                    cmd.Parameters.AddWithValue("@est", ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString());

                    cmd.ExecuteNonQuery();

                    foreach (var item in itemsDeduccion)
                    {
                        string queryDeduccion = $"UPDATE {item.TablaBD} SET Cantidad = Cantidad - @cant WHERE Nombre = @nom";
                        MySqlCommand cmdDeducir = new MySqlCommand(queryDeduccion, conexion);
                        cmdDeducir.Parameters.AddWithValue("@cant", item.Cantidad);
                        cmdDeducir.Parameters.AddWithValue("@nom", item.Nombre);
                        cmdDeducir.ExecuteNonQuery();
                    }

                    MessageBox.Show("¡Pedido guardado y stock actualizado correctamente!");
                    LimpiarFormulario();
                    CargarDatos();
                }
                catch (Exception ex) { MessageBox.Show("Error al guardar: " + ex.Message); }
            }
        }

        private void dgPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPedidos.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgPedidos.SelectedItem;

                txtFolio.Text = row["Folio"].ToString();
                txtNombre.Text = row["Nombre"].ToString();
                txtTelefono.Text = row["Telefono"].ToString();
                txtLugar.Text = row["Lugar"].ToString();
                txtHora.Text = row["Hora"].ToString();

                if (DateTime.TryParse(row["Dia"].ToString(), out DateTime fecha))
                {
                    dpDia.SelectedDate = fecha;
                }

                string estadoDb = row["Estado"].ToString() ?? "";
                foreach (ComboBoxItem item in cmbEstado.Items)
                {
                    if (item.Content.ToString() == estadoDb)
                    {
                        cmbEstado.SelectedItem = item;
                        break;
                    }
                }

                LimpiarCarrito();

                string comidaGuardada = row["Comida"].ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(comidaGuardada))
                {
                    string[] productos = comidaGuardada.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string prod in productos)
                    {
                        productosEnCarrito.Add(prod);
                        lbCarrito.Items.Add("✔️ " + prod);
                    }
                }

                totalAcumulado = Convert.ToDecimal(row["Total"]);
                lblTotal.Text = totalAcumulado.ToString("C");
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
            itemsDeduccion.Clear(); 
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

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFolio.Text))
            {
                MessageBox.Show("Primero selecciona un pedido de la tabla dando clic sobre él.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (productosEnCarrito.Count == 0)
            {
                MessageBox.Show("El carrito no puede estar vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string listaComida = string.Join(", ", productosEnCarrito);
            string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString() ?? "Pendiente";

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "UPDATE pedidos_domicilio SET Nombre=@nom, Telefono=@tel, Lugar=@lug, " +
                                   "Comida=@com, Dia=@dia, Hora=@hor, Total=@tot, Estado=@est WHERE Folio=@folio";

                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@folio", txtFolio.Text);
                    cmd.Parameters.AddWithValue("@nom", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@tel", txtTelefono.Text);
                    cmd.Parameters.AddWithValue("@lug", txtLugar.Text);
                    cmd.Parameters.AddWithValue("@com", listaComida);
                    cmd.Parameters.AddWithValue("@dia", dpDia.SelectedDate);
                    cmd.Parameters.AddWithValue("@hor", txtHora.Text);
                    cmd.Parameters.AddWithValue("@tot", totalAcumulado);
                    cmd.Parameters.AddWithValue("@est", estado);

                    cmd.ExecuteNonQuery();


                    MessageBox.Show("¡Pedido actualizado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarFormulario();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}