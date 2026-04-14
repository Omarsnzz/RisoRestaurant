using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                        {
                            catalogoCompleto.Add(new ProductoMenu { TipoBD = "alimentos", Nombre = reader["Nombre"].ToString(), Precio = Convert.ToDecimal(reader["Precio"]), Stock = Convert.ToInt32(reader["Cantidad"]) });
                        }
                    }

                    MySqlCommand cmdBebidas = new MySqlCommand("SELECT Nombre, Costo, Cantidad FROM bebidas", conexion);
                    using (MySqlDataReader reader = cmdBebidas.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            catalogoCompleto.Add(new ProductoMenu { TipoBD = "bebidas", Nombre = reader["Nombre"].ToString(), Precio = Convert.ToDecimal(reader["Costo"]), Stock = Convert.ToInt32(reader["Cantidad"]) });
                        }
                    }

                    FiltrarMenu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar el menú: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                {
                    menuFiltrado.Add(item);
                }
            }
        }

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            FiltrarMenu();
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarMenu();
        }

        private void btnAgregarLista_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un producto de la lista del menú.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cant) || cant <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida mayor a 0.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProductoMenu prod = (ProductoMenu)dgMenu.SelectedItem;

            if (cant > prod.Stock)
            {
                MessageBox.Show($"Stock insuficiente. Solo hay {prod.Stock} disponibles de {prod.Nombre}.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal subtotalItem = prod.Precio * cant;
            itemsEnCuenta.Add(new DetalleCuenta
            {
                TipoBD = prod.TipoBD,
                Nombre = prod.Nombre,
                Cantidad = cant,
                PrecioUnitario = prod.Precio,
                Subtotal = subtotalItem,
                StockMaximo = prod.Stock
            });

            ActualizarTotales();
        }

        private void dgCuenta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCuenta.SelectedItem != null)
            {
                DetalleCuenta item = (DetalleCuenta)dgCuenta.SelectedItem;
                txtEditarCantidad.Text = item.Cantidad.ToString();
            }
            else
            {
                txtEditarCantidad.Clear();
            }
        }

        private void btnActualizarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (dgCuenta.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un artículo de la cuenta para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtEditarCantidad.Text, out int nuevaCant) || nuevaCant <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida mayor a 0.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetalleCuenta item = (DetalleCuenta)dgCuenta.SelectedItem;

            if (nuevaCant > item.StockMaximo)
            {
                MessageBox.Show($"Stock insuficiente. Solo hay {item.StockMaximo} disponibles de {item.Nombre}.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            item.Cantidad = nuevaCant;
            item.Subtotal = item.PrecioUnitario * nuevaCant;

            dgCuenta.Items.Refresh();
            ActualizarTotales();
        }

        private void btnEliminarArticulo_Click(object sender, RoutedEventArgs e)
        {
            if (dgCuenta.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un artículo de la cuenta para quitarlo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetalleCuenta item = (DetalleCuenta)dgCuenta.SelectedItem;
            itemsEnCuenta.Remove(item);

            txtEditarCantidad.Clear();
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            subtotalGeneral = 0;
            foreach (var item in itemsEnCuenta)
            {
                subtotalGeneral += item.Subtotal;
            }

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
            if (decimal.TryParse(txtDescLibre.Text, out decimal desc) && desc >= 0 && desc <= 100)
            {
                porcentajeDescuentoActual = desc;
                ActualizarTotales();
            }
            else
            {
                MessageBox.Show("Ingresa un porcentaje válido (0-100).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnPropina10_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 10; ActualizarTotales(); }
        private void btnPropina15_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 15; ActualizarTotales(); }
        private void btnPropina20_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 20; ActualizarTotales(); }
        private void btnQuitarPropina_Click(object sender, RoutedEventArgs e) { porcentajePropinaActual = 0; ActualizarTotales(); }

        private void btnPropinaLibre_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPropinaLibre.Text, out decimal prop) && prop >= 0)
            {
                porcentajePropinaActual = prop;
                ActualizarTotales();
            }
            else
            {
                MessageBox.Show("Ingresa un porcentaje de propina válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnPagar_Click(object sender, RoutedEventArgs e)
        {
            if (itemsEnCuenta.Count == 0)
            {
                MessageBox.Show("No hay artículos en la cuenta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                MySqlTransaction transaccion = null;
                try
                {
                    conexion.Open();
                    transaccion = conexion.BeginTransaction();

                    decimal montoPropina = subtotalGeneral * (porcentajePropinaActual / 100);

                    string insertCuenta = "INSERT INTO cuenta (Mesa, Mesero, Subtotal, PorcentajeDescuento, Propina, Total) VALUES (@mesa, @mesero, @sub, @desc, @prop, @tot)";
                    MySqlCommand cmdCuenta = new MySqlCommand(insertCuenta, conexion, transaccion);
                    cmdCuenta.Parameters.AddWithValue("@mesa", string.IsNullOrWhiteSpace(txtMesa.Text) ? "General" : txtMesa.Text);
                    cmdCuenta.Parameters.AddWithValue("@mesero", string.IsNullOrWhiteSpace(txtMesero.Text) ? "General" : txtMesero.Text);
                    cmdCuenta.Parameters.AddWithValue("@sub", subtotalGeneral);
                    cmdCuenta.Parameters.AddWithValue("@desc", porcentajeDescuentoActual);
                    cmdCuenta.Parameters.AddWithValue("@prop", montoPropina);
                    cmdCuenta.Parameters.AddWithValue("@tot", totalFinal);
                    cmdCuenta.ExecuteNonQuery();

                    foreach (var item in itemsEnCuenta)
                    {
                        string updateInv = $"UPDATE {item.TipoBD} SET Cantidad = Cantidad - @cant WHERE Nombre = @nom";
                        MySqlCommand cmdInv = new MySqlCommand(updateInv, conexion, transaccion);
                        cmdInv.Parameters.AddWithValue("@cant", item.Cantidad);
                        cmdInv.Parameters.AddWithValue("@nom", item.Nombre);
                        cmdInv.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    MessageBox.Show($"¡Cobro realizado con éxito!\nTotal cobrado: ${totalFinal:F2}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarTodo();
                    CargarMenu();
                }
                catch (Exception ex)
                {
                    transaccion?.Rollback();
                    MessageBox.Show("Error al cobrar: " + ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarTodo();

        private void LimpiarTodo()
        {
            txtMesa.Clear();
            txtMesero.Clear();
            txtBuscar.Clear();
            txtCantidad.Text = "1";
            txtDescLibre.Clear();
            txtPropinaLibre.Clear();
            txtEditarCantidad.Clear();
            itemsEnCuenta.Clear();
            porcentajeDescuentoActual = 0;
            porcentajePropinaActual = 0;
            ActualizarTotales();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                    window.Show();
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