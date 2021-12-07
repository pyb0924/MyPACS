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
using MyPACSViewer.Model;
//using ViewerSCU;

namespace MyPACSViewer
{
    /// <summary>
    /// Interaction logic for QRConfigWindow.xaml
    /// </summary>
    public partial class QRConfigWindow : Window
    {
        public QRConfigWindow()
        {
            InitializeComponent();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }
    }
}
