using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Network;

namespace ViewerSCU
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ViewerSCU scu = new("localhost", 104, "MyPACSServer", "ViewerSCU", @".\DICOM");

            var seriesList = await scu.RunCFind("Pei^Yibo");
            for (int i = 0; i < seriesList.Count - 1; i++)
            {
                string studyInstanceUID = seriesList[i].GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
                string seriesInstanceUID = seriesList[i].GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
                await scu.RunCGet(studyInstanceUID, seriesInstanceUID);
            }
            Console.ReadLine();
        }
    }
}
