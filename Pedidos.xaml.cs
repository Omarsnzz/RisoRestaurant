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

        // NUEVA LISTA: Para llevar el control de qué vamos a descontar de la base de datos
        List<ItemDeduccion> itemsDeduccion = new List<ItemDeduccion>();

        // NUEVA CLASE INTERNA: Para estructurar la información del inventario
        private class ItemDeduccion
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public string TablaBD { get; set; } // Guardará si es "alimentos" o "bebidas"
        }

        public Pedidos()
        {
            InitializeComponent();
            CargarDatos();
            CargarMenus();
        }

        // --- CARGA DE DATOS ---
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

        // --- LÓGICA DEL CARRITO Y VALIDACIÓN DE INVENTARIO ---

        // NUEVO MÉTODO: Consulta a la BD en tiempo real para ver cuánto nos queda
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
                txtCantPlatillo.Text = "1"; // Resetear cantidad
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
                txtCantBebida.Text = "1"; // Resetear cantidad
            }
        }

        private void ProcesarIngresoAlCarrito(string nombre, decimal precioUnitario, int cantidadPedida, string tablaBD)
        {
            // 1. Verificar inventario real
            int stockTotal = ObtenerStockDisponible(nombre, tablaBD);

            // 2. Verificar cuánto de ese producto ya metimos al carrito en este mismo pedido
            int cantidadYaEnCarrito = itemsDeduccion.Where(x => x.Nombre == nombre).Sum(x => x.Cantidad);

            // 3. Validar si nos alcanza
            if (cantidadPedida + cantidadYaEnCarrito > stockTotal)
            {
                int disponibleParaAgregar = stockTotal - cantidadYaEnCarrito;
                MessageBox.Show($"No hay suficientes existencias de '{nombre}'.\n\nStock total: {stockTotal}\nYa en carrito: {cantidadYaEnCarrito}\nPuedes agregar máximo: {disponibleParaAgregar}", "Inventario Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Si todo está bien, calculamos subtotal y agregamos
            decimal subtotal = precioUnitario * cantidadPedida;
            string descripcionItem = $"{cantidadPedida} x {nombre}";

            productosEnCarrito.Add(descripcionItem);
            lbCarrito.Items.Add($"{descripcionItem} - {subtotal:C}");

            totalAcumulado += subtotal;
            lblTotal.Text = totalAcumulado.ToString("C");

            // 5. Lo guardamos en nuestra lista de control para descontarlo al final
            itemsDeduccion.Add(new ItemDeduccion { Nombre = nombre, Cantidad = cantidadPedida, TablaBD = tablaBD });
        }

        // --- GUARDADO EN BASE DE DATOS ---
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

                    // 1. Guardar el Pedido
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

                    // 2. DESCONTAR DEL INVENTARIO LOS PRODUCTOS COMPRADOS
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

        // --- OTROS BOTONES ---
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
            itemsDeduccion.Clear(); // LIMPIAMOS TAMBIÉN LA LISTA DE INVENTARIO
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

                    // Nota: Al editar no estamos tocando inventarios para no complicar la lógica si borras o agregas cosas a un pedido viejo.

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