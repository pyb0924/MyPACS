using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ViewerSCU
{
    internal class TestViewerSCU
    {
        public static async Task Main()
        {
            ViewerSCU scu = new("localhost",104,"STORESCP","ViewerSCU");
            Dictionary<string, List<string>> seriesDict = await scu.RunCFind("Pei^Yibo");
            foreach(string studyUID in seriesDict.Keys)
            {
                foreach (string seriesUID in seriesDict[studyUID])
                {
                    await scu.RunCGet(studyUID, seriesUID);
                }
            }
        }
    }
}
