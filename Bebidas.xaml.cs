using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Riso
{
    /// <summary>
    /// Lógica de interacción para Bebidas.xaml
    /// </summary>
    public partial class Bebidas : Window
    {
        public Bebidas()
        {
            InitializeComponent();
        }

        // ¡Aquí adentro va tu botón, antes de que se cierre la llave de la clase!
        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Volvemos a abrir el menú principal
            MainWindow menu = new MainWindow();
            menu.Show();

            // Cerramos la ventana de bebidas
            this.Close();
        }
    }
}