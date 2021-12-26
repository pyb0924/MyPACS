using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;
using FellowOakDicom;
using MyPACSViewer.Model;
using MyPACSViewer.Utils;
using System.Linq;

namespace MyPACSViewer.ViewModel
{
    class FileExplorerViewModel : ViewModelBase
    {
        private readonly Dictionary<string, FileNodeModel> _DicomDict = new();
        private ObservableCollection<FileNodeModel> _fileTreeDataList;
        public ObservableCollection<FileNodeModel> FileTreeDataList
        {
            get => _fileTreeDataList;
            set
            {
                _fileTreeDataList = value;
                RaisePropertyChanged(() => FileTreeDataList);
            }
        }
        public FileExplorerViewModel()
        {
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_file, async (file) =>
            {
                _DicomDict.Clear();
                await GenerateFromFile(file);
            });
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_folder, async (folder) =>
            {
                _DicomDict.Clear();
                var fileCount = await GenerateFromFolder(folder);
                ShowOpenFolderInfo(fileCount);
            });
            Messenger.Default.Register<List<string>>(this, Properties.Resources.messageKey_series, async (folders) =>
            {
                _DicomDict.Clear();
                var fileCount = await GenerateFromSeries(folders);
                ShowOpenFolderInfo(fileCount);
            });
        }

        private async Task<bool> AddToDicomDict(FileInfo file, bool selected)
        {
            string tmp;
            int index;
            DicomFile dcmFile;
            DicomDataset dcmDataSet;
            FileNodeModel patientNode, studyNode, seriesNode, imageNode;
            try
            {
                dcmFile = await DicomFile.OpenAsync(file.FullName);
                dcmDataSet = dcmFile.Dataset;
                tmp = dcmDataSet.GetString(DicomTag.PatientID);
                if (_DicomDict.ContainsKey(tmp))
                {
                    patientNode = _DicomDict[tmp];
                }
                else
                {
                    patientNode = new(dcmDataSet.GetSingleValueOrDefault(DicomTag.PatientName, tmp), Properties.Resources.patientIcon);
                    patientNode.IsExpanded = selected;
                    _DicomDict.Add(tmp, patientNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.StudyInstanceUID);
                if (patientNode.Children.ContainsKey(tmp))
                {
                    studyNode = patientNode.Children[tmp];
                }
                else
                {
                    studyNode = new(dcmDataSet.GetSingleValueOrDefault(DicomTag.Modality, string.Empty), Properties.Resources.studyIcon);
                    studyNode.IsExpanded = selected;
                    patientNode.Children.Add(tmp, studyNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.SeriesInstanceUID);
                if (studyNode.Children.ContainsKey(tmp))
                {
                    seriesNode = studyNode.Children[tmp];
                }
                else
                {
                    seriesNode = new(dcmDataSet.GetSingleValueOrDefault(DicomTag.SeriesDescription,
                        Properties.Resources.noSeriesDescriptionStr), Properties.Resources.seriesIcon);
                    seriesNode.IsExpanded = selected;
                    studyNode.Children.Add(tmp, seriesNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.SOPInstanceUID);
                if (seriesNode.Children.ContainsKey(tmp))
                {
                    imageNode = seriesNode.Children[tmp];
                }
                else
                {
                    imageNode = new(file.Name, Properties.Resources.imageIcon, file.FullName,
                        dcmDataSet.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0));
                    imageNode.IsSelected = selected;
                    seriesNode.Children.Add(tmp, imageNode);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private async Task GenerateFromFile(string file)
        {
            FileInfo fileInfo = new(file);
            if (await AddToDicomDict(fileInfo, true))
            {
                FileTreeDataList = new(_DicomDict.Values);
                Messenger.Default.Send("Open File Successfully!", Properties.Resources.messageKey_status);
            }
            else
            {
                Messenger.Default.Send("Open File Failed!", Properties.Resources.messageKey_status);
            }
        }

        private async Task<FileCountModel> GenerateFromFolder(string folder)
        {
            DirectoryInfo dir = new(folder);
            FileInfo[] files = dir.GetFiles("*.dcm", SearchOption.AllDirectories);
            var openFileTaskList = new List<Task<bool>>();
            foreach (var file in files)
            {
                openFileTaskList.Add(AddToDicomDict(file, false));
            }
            List<bool> resultList = new(await Task.WhenAll(openFileTaskList));

            FileTreeDataList = new(_DicomDict.Values);

            FileCountModel fileCount = new();
            foreach (var patientNode in _DicomDict.Values)
            {
                fileCount.PatientCount++;
                foreach (var studyNode in patientNode.Children.Values)
                {
                    fileCount.StudyCount++;
                    foreach (var seriesNode in studyNode.Children.Values)
                    {
                        fileCount.SeriesCount++;
                        seriesNode.Children = seriesNode.Children
                            .OrderBy(item => item.Value.Index)
                            .ToDictionary(item => item.Key, item => item.Value);
                    }
                }
            }
            fileCount.InstanceCount = resultList.Count(item => item);
            return fileCount;
        }

        private async Task<FileCountModel> GenerateFromSeries(List<string> folders)
        {
            List<Task<FileCountModel>> openSeriesTaskList = new();
            foreach (var folder in folders)
            {
                openSeriesTaskList.Add(GenerateFromFolder(folder));
            }
            List<FileCountModel> fileCountList = new(await Task.WhenAll(openSeriesTaskList));

            return new FileCountModel()
            {
                PatientCount = fileCountList.Sum(item => item.PatientCount),
                StudyCount = fileCountList.Sum(item => item.StudyCount),
                SeriesCount = fileCountList.Sum(item => item.SeriesCount),
                InstanceCount = fileCountList.Sum(item => item.InstanceCount),
            };
        }

        private void ShowOpenFolderInfo(FileCountModel fileCount)
        {
            Messenger.Default.Send($"Open DICOM Files Successfully! Total: {fileCount.PatientCount} Patients, " +
                $"{fileCount.StudyCount} Study, {fileCount.SeriesCount} Series, {fileCount.InstanceCount} Instances",
                Properties.Resources.messageKey_status);
        }

        public ICommand SelectedItemChangedCommand => new RelayCommand<FileNodeModel>((selectedItem) =>
        {
            if (selectedItem.Path is null || selectedItem.Index == -1)
            {
                return;
            }
            DicomDataset dataset = DicomFile.Open(selectedItem.Path).Dataset;
            FileNodeModel seriesNode = _DicomDict[dataset.GetString(DicomTag.PatientID)]
                .Children[dataset.GetString(DicomTag.StudyInstanceUID)]
                .Children[dataset.GetString(DicomTag.SeriesInstanceUID)];

            RenderSeriesMessage message = new(seriesNode, dataset.GetString(DicomTag.SOPInstanceUID));

            Messenger.Default.Send(message, Properties.Resources.messageKey_selectedChange);
        });
    }
}
