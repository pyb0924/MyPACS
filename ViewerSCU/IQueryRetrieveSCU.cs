using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewerSCU
{
    internal interface IQueryRetrieveSCU
    {
        // C-Find
        Task<Dictionary<string, List<string>>> RunCFind(string patientName);

        // C-Get
        Task RunCGet(string studyUID, string seriesUID);
    }
}
