using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MyPACSViewer.Model;
using MyPACSViewer.Utils;
using System.Threading.Tasks;

namespace MyPACSViewer.ViewModel
{
    class QRViewModel : ViewModelBase
    {

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
            string path = ConfigurationManager.AppSettings["image"];
            scu = new(host, port, server, aet, path);
            _findResultList = new();
        }


        public ICommand FindCommand => new RelayCommand(async () =>
        {
            bool useID = SelectedSearchField == Properties.Resources.patientIDStr;
            _findResultList.Clear();
            try
            {
                var findDatasetList = await scu.RunCFind(SearchText, useID);
                for (int i = 0; i < findDatasetList.Count - 1; i++)
                {
                    _findResultList.Add(new QRListItem(findDatasetList[i]));
                }
                Messenger.Default.Send($"C-Find Completed: Found {findDatasetList.Count - 1} Series in DB",
                    Properties.Resources.messageKey_status);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send("Run C-Find Failed: " + ex.Message, Properties.Resources.messageKey_status);
            }

        });

        public ICommand GetAllCommand => new RelayCommand(async () =>
        {
            if (FindResultList.Count == 0)
            {
                Messenger.Default.Send("Empty Series List to Run C-Find", Properties.Resources.messageKey_status);
                return;
            }
            
            try
            {
                Messenger.Default.Send($"Retrieving Files...", Properties.Resources.messageKey_status);
                //List<Task<string>> CGetTaskList = new();
                //foreach (var result in FindResultList)
                //{
                //    CGetTaskList.Add(scu.RunCGet(result.StudyInstanceUID, result.SeriesInstanceUID));
                //}
                //List<string> seriesPathList = new(await Task.WhenAll(CGetTaskList));
                List<string> seriesPathList = new();
                foreach (var result in FindResultList)
                {
                    seriesPathList.Add(await scu.RunCGet(result.StudyInstanceUID, result.SeriesInstanceUID));
                }

                Messenger.Default.Send(seriesPathList, Properties.Resources.messageKey_series);
                Messenger.Default.Send($"{seriesPathList.Count} Series Retrieved", Properties.Resources.messageKey_status);
                Messenger.Default.Send("Close", Properties.Resources.messageKey_close);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send("Run C-Get Failed: " + ex.Message, Properties.Resources.messageKey_status);
            }

        });

        public ICommand GetSelectedCommand => new RelayCommand(async () =>
        {
            if (FindResultList.Count == 0)
            {
                Messenger.Default.Send("Error: Empty Series List to Run C-Find", Properties.Resources.messageKey_status);
                return;
            }
            try
            {
                Messenger.Default.Send($"Retrieving Files...", Properties.Resources.messageKey_status);
                List<string> seriesPathList = new();
                foreach (var result in FindResultList)
                {
                    if (result.IsSelected)
                    {
                        seriesPathList.Add(await scu.RunCGet(result.StudyInstanceUID, result.SeriesInstanceUID));
                    }
                }
                Messenger.Default.Send(seriesPathList, Properties.Resources.messageKey_series);
                Messenger.Default.Send($"{seriesPathList.Count} Series Retrieved", Properties.Resources.messageKey_status);
                Messenger.Default.Send("Close", Properties.Resources.messageKey_close);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send("Run C-Get Failed: " + ex.Message, Properties.Resources.messageKey_status);
            }
        });

        public ICommand CancelCommand => new RelayCommand(() => Messenger.Default.Send("Close", Properties.Resources.messageKey_close));
    }
}
