using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class VentanaBebidas : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=risorestaurant;";

        public VentanaBebidas()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                   string query = "SELECT Nombre, Costo, Cantidad FROM bebidas";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgBebidas.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la base de datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAgregarBebida_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtCosto.Text) || string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                MessageBox.Show("Por favor, llena todos los campos (Nombre, Costo y Cantidad).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCosto.Text, out int costo) || !int.TryParse(txtCantidad.Text, out int cantidad))
            {
                MessageBox.Show("El costo y la cantidad deben ser números válidos.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "INSERT INTO bebidas (Nombre, Costo, Cantidad) VALUES (@nombre, @costo, @cantidad)";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@costo", costo);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Bebida agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditarBebida_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona una bebida de la tabla para editarla.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCosto.Text, out int costo) || !int.TryParse(txtCantidad.Text, out int cantidad))
            {
                MessageBox.Show("El costo y la cantidad deben ser números válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    // SE AGREGÓ CANTIDAD AL UPDATE
                    string query = "UPDATE bebidas SET Costo = @costo, Cantidad = @cantidad WHERE Nombre = @nombre";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@costo", costo);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        MessageBox.Show("Bebida actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarDatos();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró la bebida para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al editar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEliminarBebida_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona una bebida de la tabla para eliminarla.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show($"¿Estás seguro de que deseas eliminar la bebida '{txtNombre.Text}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM bebidas WHERE Nombre = @nombre";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Bebida eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        LimpiarCampos();
                        CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnLimpiarTodo_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmacion = MessageBox.Show(
                "⚠️ ADVERTENCIA CRÍTICA ⚠️\n\n¿Estás COMPLETAMENTE SEGURO de que deseas BORRAR TODAS las bebidas del menú?\n\nEsta acción NO se puede deshacer.",
                "Confirmar Limpieza Total",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM bebidas";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Se han eliminado todas las bebidas de la base de datos exitosamente.", "Limpieza Completa", MessageBoxButton.OK, MessageBoxImage.Information);

                        LimpiarCampos();
                        CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al intentar borrar todos los datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgBebidas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBebidas.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgBebidas.SelectedItem;
                txtNombre.Text = row["Nombre"].ToString();
                txtCosto.Text = row["Costo"].ToString();

                txtCantidad.Text = row["Cantidad"].ToString();
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtCosto.Clear();
            txtCantidad.Clear(); 
            dgBebidas.SelectedItem = null;
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    window.Show();
                }
            }
            this.Close();
        }
    }
}