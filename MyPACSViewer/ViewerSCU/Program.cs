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
            ViewerSCU scu = new("localhost", 104, "MyPACSServer", "ViewerSCU", @"\DICOM");

            var seriesList = await scu.RunCFind("Pei^Yibo");
            //List<Task<string>> CGetTaskList = new();
            for (int i = 0; i < seriesList.Count - 1; i++)
            {
                string studyInstanceUID = seriesList[i].GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
                string seriesInstanceUID = seriesList[i].GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
                //CGetTaskList.Add(scu.RunCGet(studyInstanceUID, seriesInstanceUID));
                Console.WriteLine(scu.RunCGet(studyInstanceUID, seriesInstanceUID));
            }
            //List<string> CGetSeriesList = new(await Task.WhenAll(CGetTaskList));
            //foreach (var seriesPath in CGetSeriesList)
            //{
            //    Console.WriteLine(seriesPath);
            //}
            Console.ReadLine();

        }
    }
}
