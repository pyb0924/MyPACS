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

namespace MyPACSViewer.ViewModel
{
    class Viewer2DViewModel : ViewModelBase
    {
        public FileNodeModel SeriesNode { get; set; }
        private DicomDataset _mainDataset;
        private DicomDataset _maskDataset;
        private bool _isMaskMode;

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

        private WriteableBitmap _maskImage;
        public WriteableBitmap MaskImage
        {
            get => _maskImage;
            set
            {
                _maskImage = value;
                RaisePropertyChanged(() => MaskImage);
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
            Messenger.Default.Register<RenderSeriesMessage>(this, Properties.Resources.messageKey_selectedChange, OnSeriesChange);
            Messenger.Default.Register<bool>(this, Properties.Resources.messageKey_detect, OnChangeMask);
            SliderValue = 1;
            SliderMax = 10;
            _isMaskMode = false;
        }

        private void RenderImage()
        {
            DicomImage image = new(_mainDataset);
            if(_isMaskMode)
            {
            }
            MainImage = image.RenderImage().As<WriteableBitmap>();
           
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
                $"Images: {SliderValue}/{SliderMax}\n" +
                $"Series: {_mainDataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, string.Empty)}";

            RightBottomText = $"WL: {_mainDataset.GetSingleValueOrDefault(DicomTag.WindowCenter, 0)} " +
                $"WW: {_mainDataset.GetSingleValueOrDefault(DicomTag.WindowWidth, 0)}";
        }

        private void Render()
        {
            RenderImage();
            RenderCornerInfo();
            Messenger.Default.Send($"Rendered Image {SliderValue}/{SliderMax}", Properties.Resources.messageKey_status);
        }

        private async void OnChangeMask(bool maskMode)
        {
            _isMaskMode = maskMode;
            string studyInstanceUID = _mainDataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
            string seriesInstanceUID = _mainDataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
            string path = ConfigurationManager.AppSettings["mask"];
            string seriesMaskDir = $@"{path}\{studyInstanceUID}\{seriesInstanceUID}";
            DirectoryInfo directoryInfo = new(seriesMaskDir);
            
            if (_isMaskMode)
            {
                if (!directoryInfo.Exists || !(directoryInfo.GetFiles().Length == SeriesNode.Children.Count))
                {
                    string host = ConfigurationManager.AppSettings["host"];
                    int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                    string server = ConfigurationManager.AppSettings["server"];
                    string aet = ConfigurationManager.AppSettings["aet"];

                    ViewerSCU.ViewerSCU scu = new(host, port, server, aet, path);
                    await scu.RunCGet(studyInstanceUID, seriesInstanceUID, true);
                }
                _maskDataset = DicomFile.Open(seriesMaskDir + $@"\{_mainDataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty)}").Dataset;                
            }
            RenderImage();
        }
        private void OnSeriesChange(RenderSeriesMessage message)
        {
            SeriesNode = message.SeriesNode;
            SliderMax = SeriesNode.Children.Count - 1;
            SliderValue = message.Index;

            var query = from node in SeriesNode.Children.Values where node.Index == message.Index select node;
            FileNodeModel imageNode = query.First();
            _mainDataset = DicomFile.Open(imageNode.Path).Dataset;
            Render();
        }

        public ICommand IndexChangeCommand => new RelayCommand(() =>
        {
            var query = from node in SeriesNode.Children.Values where node.Index == SliderValue select node;
            FileNodeModel imageNode = query.First();
            _mainDataset = DicomFile.Open(imageNode.Path).Dataset;
            Render();
        });
    }
}
