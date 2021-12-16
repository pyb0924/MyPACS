using MyPACSViewer.Model;

namespace MyPACSViewer.Utils
{
    class RenderSeriesMessage
    {
        public FileNodeModel SeriesNode { get; set; }
        public int Index { get; set; }

        public RenderSeriesMessage(FileNodeModel seriesNode,int index)
        {
            SeriesNode = seriesNode;
            Index = index;
        }
    }
}
