using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;
using FellowOakDicom.Imaging;
using FellowOakDicom;
using MyPACSViewer.Model;
using MyPACSViewer.Message;

namespace MyPACSViewer.ViewModel
{
    class Viewer2DViewModel : ViewModelBase
    {
        public FileNodeModel SeriesNode { get; set; }
        private DicomDataset _dataset;
        private bool hasMask = false;

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
            SliderValue = 1;
            SliderMax = 10;
        }

        private void Render(FileNodeModel imageNode)
        {
            _dataset = DicomFile.Open(imageNode.Path).Dataset;
            // render image
            DicomImage image = new(_dataset);
            MainImage = image.RenderImage().As<WriteableBitmap>();

            // render corner info
            LeftTopText = $"{_dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)}\n" +
                $"{_dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty)} " +
                $"{_dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty)}\n" +
                $"{_dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, string.Empty)}\n";

            RightTopText = $"{_dataset.GetSingleValueOrDefault(DicomTag.ManufacturerModelName, string.Empty)}\n" +
                $"{_dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty)} " +
                $"{_dataset.GetSingleValueOrDefault(DicomTag.StudyTime, string.Empty)}";

            LeftBottomText = $"{_dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty)}\n" +
                $"Images: {SliderValue}/{SliderMax}\n" +
                $"Series: {_dataset.GetSingleValueOrDefault(DicomTag.SeriesNumber, string.Empty)}";

            RightBottomText = $"WL: {_dataset.GetSingleValueOrDefault(DicomTag.WindowCenter, 0)} " +
                $"WW: {_dataset.GetSingleValueOrDefault(DicomTag.WindowWidth, 0)}";

            if (hasMask)
            {
                // TODO render mask
            }

            Messenger.Default.Send($"Rendered Image {SliderValue}/{SliderMax}", Properties.Resources.messageKey_status);
        }

        private void OnSeriesChange(RenderSeriesMessage message)
        {
            SeriesNode = message.SeriesNode;
            SliderMax = SeriesNode.Children.Count - 1;
            SliderValue = message.Index;

            var query = from node in SeriesNode.Children.Values where node.Index == message.Index select node;
            FileNodeModel imageNode = query.First();
            Render(imageNode);
        }

        public ICommand IndexChangeCommand => new RelayCommand(() =>
        {
            var query = from node in SeriesNode.Children.Values where node.Index == SliderValue select node;
            FileNodeModel imageNode = query.First();
            Render(imageNode);
        });
    }
}
