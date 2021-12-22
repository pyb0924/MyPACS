using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;
using FellowOakDicom.Imaging;
using FellowOakDicom;
using MyPACSViewer.Model;
using MyPACSViewer.Utils;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;

namespace MyPACSViewer.ViewModel
{
    class Viewer2DViewModel : ViewModelBase
    {
        public FileNodeModel SeriesNode { get; set; }
        private DicomDataset _mainDataset;
        private bool _isAnnotationMode;

        #region Properties
        private WriteableBitmap _mainImage;
        public WriteableBitmap MainImage
        {
            get => _mainImage;
            set
            {
                _mainImage = value;
                RaisePropertyChanged(() => MainImage);
            }
        }

        private string _leftTopText;
        public string LeftTopText
        {
            get => _leftTopText;
            set
            {
                _leftTopText = value;
                RaisePropertyChanged(() => LeftTopText);
            }
        }

        private string _leftBottomText;
        public string LeftBottomText
        {
            get => _leftBottomText;
            set
            {
                _leftBottomText = value;
                RaisePropertyChanged(() => LeftBottomText);
            }
        }

        private string _rightTopText;
        public string RightTopText
        {
            get => _rightTopText;
            set
            {
                _rightTopText = value;
                RaisePropertyChanged(() => RightTopText);
            }
        }

        private string _rightBottomText;
        public string RightBottomText
        {
            get => _rightBottomText;
            set
            {
                _rightBottomText = value;
                RaisePropertyChanged(() => RightBottomText);
            }
        }

        private int _sliderMin;
        public int SliderMin
        {
            get => _sliderMin;
            set
            {
                _sliderMin = value;
                RaisePropertyChanged(() => SliderMin);
            }
        }

        private int _sliderMax;
        public int SliderMax
        {
            get => _sliderMax;
            set
            {
                _sliderMax = value;
                RaisePropertyChanged(() => SliderMax);
            }
        }

        private int _sliderValue;
        public int SliderValue
        {
            get => _sliderValue;
            set
            {
                _sliderValue = value;
                RaisePropertyChanged(() => SliderValue);
            }
        }

        #endregion

        public Viewer2DViewModel()
        {
            new DicomSetupBuilder().RegisterServices(s => s.AddImageManager<WPFImageManager>()).Build();
            Messenger.Default.Register<RenderSeriesMessage>(this, Properties.Resources.messageKey_selectedChange, OnSelectedChange);
            Messenger.Default.Register<bool>(this, Properties.Resources.messageKey_detect, OnChangeAnnotationMode);
            _isAnnotationMode = false;
        }

        private void RenderImage()
        {
            DicomImage image = new(_mainDataset);
            image.ShowOverlays = _isAnnotationMode;
            MainImage = image.RenderImage().As<WriteableBitmap>();
            Messenger.Default.Send($"Rendered Image {SliderValue - SliderMin + 1}/{SliderMax - SliderMin + 1}, " +
                $"Show Annotation={_isAnnotationMode}", Properties.Resources.messageKey_status);
        }

        private void RenderCornerInfo()
        {
            LeftTopText = $"{_mainDataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)}\n" +
                $"{_mainDataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty)} " +
                $"{_mainDataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty)}\n" +
                $"{_mainDataset.GetSingleValueOrDefault(DicomTag.StudyDescription, string.Empty)}\n";

            RightTopText = $"{_mainDataset.GetSingleValueOrDefault(DicomTag.ManufacturerModelName, string.Empty)}\n" +
                $"{_mainDataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty)} " +
                $"{_mainDataset.GetSingleValueOrDefault(DicomTag.StudyTime, string.Empty)}";

            LeftBottomText = $"{_mainDataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty)}\n" +
                $"Images: {SliderValue - SliderMin + 1}/{SliderMax - SliderMin + 1}\n" +
                $"Series: {_mainDataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, string.Empty)}";

            RightBottomText = $"WL: {_mainDataset.GetSingleValueOrDefault(DicomTag.WindowCenter, 0)} " +
                $"WW: {_mainDataset.GetSingleValueOrDefault(DicomTag.WindowWidth, 0)}";
        }

        private void Render()
        {
            RenderImage();
            RenderCornerInfo();
        }

        private async Task GetDatasetWithOverlay()
        {
            string studyInstanceUID = _mainDataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
            string seriesInstanceUID = _mainDataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            string sopInstanceUID = _mainDataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);
            string annotationRoot = ConfigurationManager.AppSettings["annotation"];
            string filePath = $@"{annotationRoot}\{studyInstanceUID}\{seriesInstanceUID}\{sopInstanceUID + Properties.Resources.dicomExt}";
            FileInfo fileInfo = new(filePath);

            if (!fileInfo.Exists)
            {
                string host = ConfigurationManager.AppSettings["host"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string server = ConfigurationManager.AppSettings["server"];
                string aet = ConfigurationManager.AppSettings["aet"];

                ViewerSCU.ViewerSCU scu = new(host, port, server, aet, annotationRoot);
                Messenger.Default.Send("Retriving Data with Annotation from Server...", Properties.Resources.messageKey_status);
                await scu.RunCGet(studyInstanceUID, seriesInstanceUID, true);

            }
            _mainDataset = DicomFile.Open(filePath).Dataset;
        }

        private async void OnChangeAnnotationMode(bool isAnnotationMode)
        {
            if (_mainDataset is null)
            {
                Messenger.Default.Send("No Image Rendered!", Properties.Resources.messageKey_status);
                return;
            }

            if (isAnnotationMode)
            {
                string overlayType = _mainDataset.GetSingleValueOrDefault(DicomTag.OverlayType, string.Empty);
                if (string.IsNullOrEmpty(overlayType))
                {
                    try
                    {
                        await GetDatasetWithOverlay();
                        _isAnnotationMode = isAnnotationMode;
                        RenderImage();
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send($"C-GET Error: {ex.Message}", Properties.Resources.messageKey_status);
                    }
                }
            }
            else
            {
                _isAnnotationMode = isAnnotationMode;
                RenderImage();
            }

            Messenger.Default.Send(_isAnnotationMode, Properties.Resources.messageKey_annotationModeChange);

        }
        private async void OnSelectedChange(RenderSeriesMessage message)
        {
            SeriesNode = message.SeriesNode;
            var imageNode = SeriesNode.Children[message.SOPInstanceUID];

            SliderMin = SeriesNode.Children.Min(image => image.Value.Index);
            SliderMax = SeriesNode.Children.Max(image => image.Value.Index);
            if (SliderValue == imageNode.Index)
            {
                _mainDataset = DicomFile.Open(imageNode.Path).Dataset;
                if (_isAnnotationMode)
                {
                    await GetDatasetWithOverlay();
                }
                Render();
                Messenger.Default.Send(_isAnnotationMode, Properties.Resources.messageKey_annotationModeChange);
            }
            else
            {
                SliderValue = imageNode.Index;
            }
        }

        public ICommand IndexChangeCommand => new RelayCommand(async () =>
        {
            if (SliderMax - SliderMin + 1 != SeriesNode.Children.Count)
            {
                return;
            }
            var query = from node in SeriesNode.Children.Values where node.Index == SliderValue select node;
            FileNodeModel imageNode = query.First();
            _mainDataset = DicomFile.Open(imageNode.Path).Dataset;
            if (_isAnnotationMode)
            {
                try
                {
                    await GetDatasetWithOverlay();
                    Render();
                }
                catch (Exception ex)
                {
                    Messenger.Default.Send($"Error Running C-GET(with annotation): {ex.Message}", Properties.Resources.messageKey_status);
                }
            }
            else
            {
                Render();
            }
        });
    }
}
