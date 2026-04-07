using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Riso
{
    public partial class VentanaInventario : Window
    {
        string cadenaConexion = "server=localhost;port=3306;user=root;password=;database=risorestaurant;";

        public VentanaInventario()
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
                    string query = "SELECT Nombre, Cantidad, UnidadMedida FROM inventario";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgInventario.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar el inventario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtCantidad.Text) || string.IsNullOrWhiteSpace(txtUnidad.Text))
            {
                MessageBox.Show("Por favor, llena todos los campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad))
            {
                MessageBox.Show("La cantidad debe ser un número (puedes usar decimales).", "Error de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "INSERT INTO inventario (Nombre, Cantidad, UnidadMedida) VALUES (@nombre, @cantidad, @unidad)";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@unidad", txtUnidad.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Artículo agregado al inventario.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona un artículo de la tabla para editarlo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad))
            {
                MessageBox.Show("La cantidad debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "UPDATE inventario SET Cantidad = @cantidad, UnidadMedida = @unidad WHERE Nombre = @nombre";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@unidad", txtUnidad.Text);

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        MessageBox.Show("Inventario actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarDatos();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el artículo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al editar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Selecciona un artículo para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show($"¿Eliminar '{txtNombre.Text}' del inventario?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM inventario WHERE Nombre = @nombre";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);
                        cmd.Parameters.AddWithValue("@nombre", txtNombre.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Artículo eliminado del inventario.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

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
            MessageBoxResult confirmacion = MessageBox.Show("¿Seguro que deseas VACIAR TODO EL INVENTARIO?\nEsta acción no se puede deshacer.", "Vaciar Bodega", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
                {
                    try
                    {
                        conexion.Open();
                        string query = "DELETE FROM inventario";
                        MySqlCommand cmd = new MySqlCommand(query, conexion);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("El inventario ha sido vaciado.", "Limpieza Completa", MessageBoxButton.OK, MessageBoxImage.Information);
                        LimpiarCampos();
                        CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al limpiar inventario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (dgInventario.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgInventario.SelectedItem;
                txtNombre.Text = row["Nombre"].ToString();
                txtCantidad.Text = row["Cantidad"].ToString();
                txtUnidad.Text = row["UnidadMedida"].ToString();
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtCantidad.Clear();
            txtUnidad.Clear();
            dgInventario.SelectedItem = null;
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