using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class Cuenta : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=risorestaurant;";

        List<ProductoMenu> catalogoCompleto = new List<ProductoMenu>();
        ObservableCollection<ProductoMenu> menuFiltrado = new ObservableCollection<ProductoMenu>();

        ObservableCollection<DetalleCuenta> itemsEnCuenta = new ObservableCollection<DetalleCuenta>();
        List<DetalleCuenta> itemsOriginalesParaRevertirStock = new List<DetalleCuenta>();

        decimal subtotalGeneral = 0;
        decimal porcentajeDescuentoActual = 0;
        decimal porcentajePropinaActual = 0;
        decimal totalFinal = 0;

        public Cuenta()
        {
            InitializeComponent();
            dgCuenta.ItemsSource = itemsEnCuenta;
            dgMenu.ItemsSource = menuFiltrado;
            CargarMenu();
            CargarCuentasPorFecha(DateTime.Today);
            dpFiltroFecha.SelectedDate = DateTime.Today;
        }

        private void CargarMenu()
        {
            catalogoCompleto.Clear();
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    MySqlCommand cmdAlimentos = new MySqlCommand("SELECT Nombre, Precio, Cantidad FROM alimentos", conexion);
                    using (MySqlDataReader reader = cmdAlimentos.ExecuteReader())
                    {
                        while (reader.Read())
                            catalogoCompleto.Add(new ProductoMenu { TipoBD = "alimentos", Nombre = reader["Nombre"].ToString(), Precio = Convert.ToDecimal(reader["Precio"]), Stock = Convert.ToInt32(reader["Cantidad"]) });
                    }

                    MySqlCommand cmdBebidas = new MySqlCommand("SELECT Nombre, Costo, Cantidad FROM bebidas", conexion);
                    using (MySqlDataReader reader = cmdBebidas.ExecuteReader())
                    {
                        while (reader.Read())
                            catalogoCompleto.Add(new ProductoMenu { TipoBD = "bebidas", Nombre = reader["Nombre"].ToString(), Precio = Convert.ToDecimal(reader["Costo"]), Stock = Convert.ToInt32(reader["Cantidad"]) });
                    }

                    FiltrarMenu();
                }
                catch (Exception ex) { MessageBox.Show("Error al cargar el menú: " + ex.Message); }
            }
        }

        private void FiltrarMenu()
        {
            if (catalogoCompleto == null || menuFiltrado == null || txtBuscar == null || rbAlimentos == null) return;
            string filtroTipo = rbAlimentos.IsChecked == true ? "alimentos" : "bebidas";
            string busqueda = txtBuscar.Text.ToLower();

            menuFiltrado.Clear();
            foreach (var item in catalogoCompleto)
            {
                if (item.TipoBD == filtroTipo && item.Nombre.ToLower().Contains(busqueda))
                    menuFiltrado.Add(item);
            }
        }

        private void Filtro_Changed(object sender, RoutedEventArgs e) => FiltrarMenu();
        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) => FiltrarMenu();

        private void btnAgregarLista_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem == null) { MessageBox.Show("Selecciona un producto.", "Aviso"); return; }
            if (!int.TryParse(txtCantidad.Text, out int cant) || cant <= 0) { MessageBox.Show("Cantidad inválida.", "Aviso"); return; }

            ProductoMenu prod = (ProductoMenu)dgMenu.SelectedItem;
            if (cant > prod.Stock) { MessageBox.Show($"Stock insuficiente. Hay {prod.Stock}.", "Aviso"); return; }

            itemsEnCuenta.Add(new DetalleCuenta
            {
                TipoBD = prod.TipoBD,
                Nombre = prod.Nombre,
                Cantidad = cant,
                PrecioUnitario = prod.Precio,
                Subtotal = prod.Precio * cant,
                StockMaximo = prod.Stock
            });
            ActualizarTotales();
        }

        private void dgCuenta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCuenta.SelectedItem != null)
                txtEditarCantidad.Text = ((DetalleCuenta)dgCuenta.SelectedItem).Cantidad.ToString();
            else
                txtEditarCantidad.Clear();
        }

        private void btnActualizarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (dgCuenta.SelectedItem == null) { MessageBox.Show("Selecciona un artículo para editar.", "Aviso"); return; }
            if (!int.TryParse(txtEditarCantidad.Text, out int cant) || cant <= 0) return;

            DetalleCuenta item = (DetalleCuenta)dgCuenta.SelectedItem;
            if (cant > item.StockMaximo) { MessageBox.Show($"Stock insuficiente. Hay {item.StockMaximo}.", "Aviso"); return; }

            item.Cantidad = cant;
            item.Subtotal = item.PrecioUnitario * cant;
            dgCuenta.Items.Refresh();
            ActualizarTotales();
        }

        private void btnEliminarArticulo_Click(object sender, RoutedEventArgs e)
        {
            if (dgCuenta.SelectedItem == null) return;
            itemsEnCuenta.Remove((DetalleCuenta)dgCuenta.SelectedItem);
            txtEditarCantidad.Clear();
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            subtotalGeneral = 0;
            foreach (var item in itemsEnCuenta) subtotalGeneral += item.Subtotal;

            decimal montoDescuento = subtotalGeneral * (porcentajeDescuentoActual / 100);
            decimal montoPropina = subtotalGeneral * (porcentajePropinaActual / 100);
            totalFinal = subtotalGeneral - montoDescuento + montoPropina;

            lblSubtotal.Text = $"Subtotal: ${subtotalGeneral:F2}";
            lblDescuento.Text = $"Descuento ({porcentajeDescuentoActual}%): -${montoDescuento:F2}";
            lblPropina.Text = $"Propina ({porcentajePropinaActual}%): +${montoPropina:F2}";
            lblTotal.Text = $"TOTAL: ${totalFinal:F2}";
        }

        private void btnDesc10_Click(object sender, RoutedEventArgs e) { porcentajeDescuentoActual = 10; ActualizarTotales(); }
        private void btnDesc15_Click(object sender, RoutedEventArgs e) { porcentajeDescuentoActual = 15; ActualizarTotales(); }
        private void btnQuitarDesc_Click(object sender, RoutedEventArgs e) { porcentajeDescuentoActual = 0; ActualizarTotales(); }
        private void btnDescLibre_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtDescLibre.Text, out decimal desc) && desc >= 0) { porcentajeDescuentoActual = desc; ActualizarTotales(); }
        }

        private void btnPropina10_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 10; ActualizarTotales(); }
        private void btnPropina15_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 15; ActualizarTotales(); }
        private void btnPropina20_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 20; ActualizarTotales(); }
        private void btnQuitarPropina_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 0; ActualizarTotales(); }
        private void btnPropinaLibre_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPropinaLibre.Text, out decimal prop) && prop >= 0) { porcentajePropinaActual = prop; ActualizarTotales(); }
        }

        private void CargarCuentasPorFecha(DateTime fecha)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT * FROM cuenta WHERE DATE(FechaHora) = @fecha ORDER BY idCuenta DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgCuentasGuardadas.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex) { MessageBox.Show("Error al cargar historial: " + ex.Message); }
            }
        }

        private void btnBuscarFecha_Click(object sender, RoutedEventArgs e)
        {
            if (dpFiltroFecha.SelectedDate.HasValue) CargarCuentasPorFecha(dpFiltroFecha.SelectedDate.Value);
        }

        private void btnVerHoy_Click(object sender, RoutedEventArgs e)
        {
            dpFiltroFecha.SelectedDate = DateTime.Today;
            CargarCuentasPorFecha(DateTime.Today);
        }

        private void dgCuentasGuardadas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCuentasGuardadas.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgCuentasGuardadas.SelectedItem;
                txtIdCuenta.Text = row["idCuenta"].ToString();
                txtMesa.Text = row["Mesa"].ToString();
                txtMesero.Text = row["Mesero"].ToString();
                porcentajeDescuentoActual = Convert.ToDecimal(row["PorcentajeDescuento"]);

                if (row.DataView.Table.Columns.Contains("Propina") && row["Propina"] != DBNull.Value)
                {
                    decimal dineroPropina = Convert.ToDecimal(row["Propina"]);
                    decimal subtotalViejo = Convert.ToDecimal(row["Subtotal"]);
                    porcentajePropinaActual = subtotalViejo > 0 ? (dineroPropina / subtotalViejo) * 100 : 0;
                }

                itemsEnCuenta.Clear();
                itemsOriginalesParaRevertirStock.Clear();

                string articulosGuardados = row["Articulos"].ToString();
                if (!string.IsNullOrWhiteSpace(articulosGuardados))
                {
                    string[] listaItems = articulosGuardados.Split('~');
                    foreach (string i in listaItems)
                    {
                        string[] datos = i.Split('|');
                        if (datos.Length >= 5)
                        {
                            DetalleCuenta itemRecuperado = new DetalleCuenta
                            {
                                TipoBD = datos[0],
                                Nombre = datos[1],
                                Cantidad = int.Parse(datos[2]),
                                PrecioUnitario = decimal.Parse(datos[3]),
                                Subtotal = decimal.Parse(datos[4]),
                                StockMaximo = 999
                            };
                            itemsEnCuenta.Add(itemRecuperado);

                            itemsOriginalesParaRevertirStock.Add(new DetalleCuenta { TipoBD = datos[0], Nombre = datos[1], Cantidad = int.Parse(datos[2]) });
                        }
                    }
                }

                ActualizarTotales();
                btnGuardar.IsEnabled = false;
                btnActualizar.IsEnabled = true;
            }
        }

        private string GenerarStringArticulos()
        {
            List<string> lista = new List<string>();
            foreach (var item in itemsEnCuenta)
                lista.Add($"{item.TipoBD}|{item.Nombre}|{item.Cantidad}|{item.PrecioUnitario}|{item.Subtotal}");
            return string.Join("~", lista);
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (itemsEnCuenta.Count == 0) { MessageBox.Show("No hay artículos.", "Aviso"); return; }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                MySqlTransaction transaccion = null;
                try
                {
                    conexion.Open();
                    transaccion = conexion.BeginTransaction();
                    decimal montoPropina = subtotalGeneral * (porcentajePropinaActual / 100);
                    string articulosStr = GenerarStringArticulos();

                    string insert = "INSERT INTO cuenta (Mesa, Mesero, Subtotal, PorcentajeDescuento, Propina, Total, Articulos) VALUES (@mesa, @mesero, @sub, @desc, @prop, @tot, @arts)";
                    MySqlCommand cmd = new MySqlCommand(insert, conexion, transaccion);
                    cmd.Parameters.AddWithValue("@mesa", string.IsNullOrWhiteSpace(txtMesa.Text) ? "General" : txtMesa.Text);
                    cmd.Parameters.AddWithValue("@mesero", string.IsNullOrWhiteSpace(txtMesero.Text) ? "General" : txtMesero.Text);
                    cmd.Parameters.AddWithValue("@sub", subtotalGeneral);
                    cmd.Parameters.AddWithValue("@desc", porcentajeDescuentoActual);
                    cmd.Parameters.AddWithValue("@prop", montoPropina);
                    cmd.Parameters.AddWithValue("@tot", totalFinal);
                    cmd.Parameters.AddWithValue("@arts", articulosStr);
                    cmd.ExecuteNonQuery();

                    foreach (var item in itemsEnCuenta)
                    {
                        MySqlCommand cmdInv = new MySqlCommand($"UPDATE {item.TipoBD} SET Cantidad = Cantidad - @cant WHERE Nombre = @nom", conexion, transaccion);
                        cmdInv.Parameters.AddWithValue("@cant", item.Cantidad);
                        cmdInv.Parameters.AddWithValue("@nom", item.Nombre);
                        cmdInv.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    MessageBox.Show("Cuenta guardada exitosamente.");
                    LimpiarTodo();
                    CargarMenu();
                    CargarCuentasPorFecha(dpFiltroFecha.SelectedDate.Value);
                }
                catch (Exception ex) { transaccion?.Rollback(); MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdCuenta.Text)) { MessageBox.Show("Selecciona una cuenta del historial."); return; }
            if (itemsEnCuenta.Count == 0) { MessageBox.Show("La cuenta no puede quedar vacía."); return; }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                MySqlTransaction transaccion = null;
                try
                {
                    conexion.Open();
                    transaccion = conexion.BeginTransaction();

                    foreach (var itemViejo in itemsOriginalesParaRevertirStock)
                    {
                        MySqlCommand cmdRevertir = new MySqlCommand($"UPDATE {itemViejo.TipoBD} SET Cantidad = Cantidad + @cant WHERE Nombre = @nom", conexion, transaccion);
                        cmdRevertir.Parameters.AddWithValue("@cant", itemViejo.Cantidad);
                        cmdRevertir.Parameters.AddWithValue("@nom", itemViejo.Nombre);
                        cmdRevertir.ExecuteNonQuery();
                    }

                    foreach (var itemNuevo in itemsEnCuenta)
                    {
                        MySqlCommand cmdDescontar = new MySqlCommand($"UPDATE {itemNuevo.TipoBD} SET Cantidad = Cantidad - @cant WHERE Nombre = @nom", conexion, transaccion);
                        cmdDescontar.Parameters.AddWithValue("@cant", itemNuevo.Cantidad);
                        cmdDescontar.Parameters.AddWithValue("@nom", itemNuevo.Nombre);
                        cmdDescontar.ExecuteNonQuery();
                    }

                    decimal montoPropina = subtotalGeneral * (porcentajePropinaActual / 100);
                    string articulosStr = GenerarStringArticulos();

                    string update = "UPDATE cuenta SET Mesa=@mesa, Mesero=@mesero, Subtotal=@sub, PorcentajeDescuento=@desc, Propina=@prop, Total=@tot, Articulos=@arts WHERE idCuenta=@id";
                    MySqlCommand cmd = new MySqlCommand(update, conexion, transaccion);
                    cmd.Parameters.AddWithValue("@id", txtIdCuenta.Text);
                    cmd.Parameters.AddWithValue("@mesa", txtMesa.Text);
                    cmd.Parameters.AddWithValue("@mesero", txtMesero.Text);
                    cmd.Parameters.AddWithValue("@sub", subtotalGeneral);
                    cmd.Parameters.AddWithValue("@desc", porcentajeDescuentoActual);
                    cmd.Parameters.AddWithValue("@prop", montoPropina);
                    cmd.Parameters.AddWithValue("@tot", totalFinal);
                    cmd.Parameters.AddWithValue("@arts", articulosStr);
                    cmd.ExecuteNonQuery();

                    transaccion.Commit();
                    MessageBox.Show("Cuenta actualizada exitosamente.");
                    LimpiarTodo();
                    CargarMenu();
                    CargarCuentasPorFecha(dpFiltroFecha.SelectedDate.Value);
                }
                catch (Exception ex) { transaccion?.Rollback(); MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarTodo();

        private void LimpiarTodo()
        {
            txtIdCuenta.Clear();
            txtMesa.Clear();
            txtMesero.Clear();
            txtBuscar.Clear();
            txtCantidad.Text = "1";
            txtDescLibre.Clear();
            txtPropinaLibre.Clear();
            txtEditarCantidad.Clear();
            itemsEnCuenta.Clear();
            itemsOriginalesParaRevertirStock.Clear();
            porcentajeDescuentoActual = 0;
            porcentajePropinaActual = 0;
            ActualizarTotales();

            btnGuardar.IsEnabled = true;
            btnActualizar.IsEnabled = false;
            dgCuentasGuardadas.SelectedItem = null;
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Show();
                    break;
                }
            }
            this.Close();
        }
    }

    public class ProductoMenu
    {
        public string TipoBD { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class DetalleCuenta
    {
        public string TipoBD { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int StockMaximo { get; set; }
    }
}