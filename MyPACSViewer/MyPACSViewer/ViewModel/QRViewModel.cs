using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MyPACSViewer.Model;

namespace MyPACSViewer.ViewModel
{
    class QRViewModel : ViewModelBase
    {
        public string StorageRoot { get; set; }

        private List<string> _searchFieldList = new()
        {
            Properties.Resources.patientNameStr,
            Properties.Resources.patientIDStr
        };

        public List<string> SearchFieldList
        {
            get => _searchFieldList;
            set
            {
                _searchFieldList = value;
                RaisePropertyChanged(() => SearchFieldList);
            }
        }

        private string _selectedSearchField;
        public string SelectedSearchField
        {
            get => _selectedSearchField;
            set
            {
                _selectedSearchField = value;
                RaisePropertyChanged(nameof(SelectedSearchField));
            }
        }

        public string SearchText { get; set; }

        private readonly ViewerSCU.ViewerSCU scu;

        private ObservableCollection<QRListItem> _findResultList;
        public ObservableCollection<QRListItem> FindResultList
        {
            get => _findResultList;
            set
            {
                _findResultList = value;
                RaisePropertyChanged(nameof(FindResultList));
            }
        }

        private bool _dialogResult;
        public bool DialogResult
        {
            get => _dialogResult;
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    RaisePropertyChanged(nameof(DialogResult));
                }
            }
        }

        public QRViewModel()
        {
            string host = ConfigurationManager.AppSettings["host"];
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            string server = ConfigurationManager.AppSettings["server"];
            string aet = ConfigurationManager.AppSettings["aet"];
            StorageRoot = ConfigurationManager.AppSettings["storage"];
            scu = new(host, port, server, aet);
            _findResultList = new();
        }

        private static string GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        public ICommand FindCommand => new RelayCommand(async () =>
        {
            bool useID = SelectedSearchField == Properties.Resources.patientIDStr;
            _findResultList.Clear();
            var findDatasetList = await scu.RunCFind(SearchText, useID);
            for (int i = 0; i < findDatasetList.Count - 1; i++)
            {
                _findResultList.Add(new QRListItem(findDatasetList[i]));
            }
        });

        public ICommand GetAllCommand => new RelayCommand(async () =>
        {
            if (FindResultList.Count != 0)
            {
                string storagePath = @".\" + StorageRoot + @".\" + GetTimestamp();
                scu.StoragePath = storagePath;
                Messenger.Default.Send($"Retrieving Files...", Properties.Resources.messageKey_status);
                foreach (var result in FindResultList)
                {
                    await scu.RunCGet(result.StudyInstanceUID, result.SeriesInstanceUID);
                }
                Messenger.Default.Send(storagePath, Properties.Resources.messageKey_folder);
                Messenger.Default.Send($"{FindResultList.Count} Series Retrieved",Properties.Resources.messageKey_status);
            }

            Messenger.Default.Send("Close", Properties.Resources.messageKey_close);
        });

        public ICommand GetSelectedCommand => new RelayCommand(async () =>
        {
            if (FindResultList.Count != 0)
            {
                string storagePath = @".\" + StorageRoot + @".\" + GetTimestamp();
                scu.StoragePath = storagePath;
                int seriesCount = 0;
                Messenger.Default.Send($"Retrieving Files...", Properties.Resources.messageKey_status);
                foreach (var result in FindResultList)
                {
                    if (result.IsSelected)
                    {
                        seriesCount++;
                        await scu.RunCGet(result.StudyInstanceUID, result.SeriesInstanceUID);
                    }
                }
                Messenger.Default.Send(storagePath, Properties.Resources.messageKey_folder);
                Messenger.Default.Send($"{seriesCount} Series Retrieved", Properties.Resources.messageKey_status);
            }
            Messenger.Default.Send("Close", Properties.Resources.messageKey_close);
        });

        public ICommand CancelCommand => new RelayCommand(() => Messenger.Default.Send("Close", Properties.Resources.messageKey_close));
    }
}
