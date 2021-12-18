using MyPACSViewer.Model;

namespace MyPACSViewer.Utils
{
    class RenderSeriesMessage
    {
        public FileNodeModel SeriesNode { get; set; }
        public string  SOPInstanceUID { get; set; }

        public RenderSeriesMessage(FileNodeModel seriesNode,string sopuid)
        {
            SeriesNode = seriesNode;
            SOPInstanceUID = sopuid;
        }
    }
}
