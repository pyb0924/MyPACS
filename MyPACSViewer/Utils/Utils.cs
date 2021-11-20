using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPACSViewer.Utils
{
    internal static class Utils
    {
        public enum CompressType { NO_COMPRESS, JPEG_COMPRESS };

        public enum Display3DPlane { TRANSVERSE, CORONAL, SAGITTAL,VOLUME_RENDERING };

        public enum DisplayMode { MODE_2D, MODE_3D };

        public static DisplayMode ToggleDisplayMode(this DisplayMode mode)
        {
            DisplayMode result;
            if (mode == DisplayMode.MODE_2D)
            {
                result = DisplayMode.MODE_3D;
            }
            else
            {
                result = DisplayMode.MODE_2D;
            }
            return result;
        }

    }



}
