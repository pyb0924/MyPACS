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

namespace MyPACSViewer.Components
{
    /// <summary>
    /// Interaction logic for Viewer2D.xaml
    /// </summary>
    public partial class Viewer2D : Grid
    {
        public Viewer2D()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            FocusX = ActualWidth / 2;
            FocusY = ActualHeight / 2;
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(WriteableBitmap), typeof(Viewer2D));

        public WriteableBitmap Source
        {
            get => (WriteableBitmap)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty FocusXProperty = DependencyProperty.Register(
            "FocusX", typeof(double), typeof(Viewer2D));

        public double FocusX
        {
            get => (double)GetValue(FocusXProperty);
            set => SetValue(FocusXProperty, value);
        }

        public static readonly DependencyProperty FocusYProperty = DependencyProperty.Register(
            "FocusY", typeof(double), typeof(Viewer2D));

        public double FocusY
        {
            get => (double)GetValue(FocusYProperty);
            set => SetValue(FocusYProperty, value);
        }
    }
}
