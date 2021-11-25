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
using MyPACSViewer.DataItems;
//using ViewerSCU;

namespace MyPACSViewer.Components
{
    /// <summary>
    /// Interaction logic for QRConfigWindow.xaml
    /// </summary>
    public partial class QRConfigWindow : Window
    {
        private List<QRListItem> QRItemList;
        //private ViewerSCU.ViewerSCU scu;
        public QRConfigWindow()
        {
            InitializeComponent();
            //scu = new("Utils/App.config");
            QRItemList = new();
        }

        private async void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            //Dictionary<string, List<string>> seriesDict = await scu.RunCFind(PatientNameBox.Text);
            Dictionary<string, List<string>> seriesDict = new();
            for(int i=0;i<10;i++)
            {
                var tmp = new List<string>();
                for (int j=0;j<10;j++)
                {
                    tmp.Add("**"+j+"**");
                }
                seriesDict.Add(i.ToString(), tmp);
            }
            foreach (string key in seriesDict.Keys)
            {
                foreach (string item in seriesDict[key])
                {
                    QRItemList.Add(new QRListItem(true, key, item));
                }
            }
            QRListView.ItemsSource = QRItemList;
        }

        private async void GetSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = QRItemList.Count(item => item.IsSelected);
            IEnumerable<QRListItem> items = from item in QRItemList where item.IsSelected select item;
            foreach (QRListItem item in items)
            {
                //await scu.RunCGet(item.StudyUID, item.SeriesUID);
            }
            
        }

        private async void GetAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (QRListItem item in QRItemList)
            {
                //await scu.RunCGet(item.StudyUID, item.SeriesUID);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
