using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class PlatillosDia : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=risorestaurant;";

        public PlatillosDia()
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
                    string query = "SELECT Nombre, Precio, Cantidad FROM alimentos";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgPlatillos.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la base de datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btnAgregarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtPrecio.Text) || string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                MessageBox.Show("Por favor, llena todos los campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPrecio.Text, out int precio) || !int.TryParse(txtCantidad.Text, out int cantidad))
            {
                MessageBox.Show("El precio y la cantidad deben ser números enteros válidos.", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "INSERT INTO alimentos (Nombre, Precio, Cantidad) VALUES (@nombre, @precio, @cantidad)";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Platillo agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona un platillo de la tabla para editarlo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPrecio.Text, out int precio) || !int.TryParse(txtCantidad.Text, out int cantidad))
            {
                MessageBox.Show("El precio y la cantidad deben ser números.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "UPDATE alimentos SET Precio = @precio, Cantidad = @cantidad WHERE Nombre = @nombre";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        MessageBox.Show("Platillo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarDatos();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el platillo para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al editar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btnEliminarPlatillo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona un platillo de la tabla para eliminarlo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show($"¿Estás seguro de que deseas eliminar '{txtNombre.Text}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM alimentos WHERE Nombre = @nombre";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Platillo eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

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
                "⚠️ ADVERTENCIA CRÍTICA ⚠️\n\n¿Estás COMPLETAMENTE SEGURO de que deseas BORRAR TODOS los alimentos del menú?\n\nEsta acción NO se puede deshacer.",
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
                        string query = "DELETE FROM alimentos";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Se han eliminado todos los alimentos de la base de datos exitosamente.", "Limpieza Completa", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void dgPlatillos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPlatillos.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgPlatillos.SelectedItem;
                txtNombre.Text = row["Nombre"].ToString();
                txtPrecio.Text = row["Precio"].ToString();
                txtCantidad.Text = row["Cantidad"].ToString();
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtCantidad.Clear();
            dgPlatillos.SelectedItem = null;
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