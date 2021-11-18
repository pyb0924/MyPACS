using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;


namespace ViewerSCU
{
    public class ViewerSCU:IQueryRetrieveSCU
    {
        private const string _StoragePath = @".\DICOM";
        private static readonly string _Encoding = "IOS_IR 100";

        public string Host { get; } = "localhost";
        public int Port { get; } = 104;
        public string ServerAET { get; } = "MyPACSServer";
        public string Aet { get; } = "ViewerSCU";
        private readonly IDicomClient Client;

        public ViewerSCU()
        {
            Client = DicomClientFactory.Create(Host, Port, false, Aet, ServerAET);
            Client.NegotiateAsyncOps();
        }

        public ViewerSCU(string host, int port, string serverAET, string aet)
        {
            Host = host;
            Port = port;
            ServerAET = serverAET;
            Aet = aet;
            Client = DicomClientFactory.Create(Host, Port, false, Aet, ServerAET);
            Client.NegotiateAsyncOps();
        }

        public async Task<Dictionary<string, List<string>>> RunCFind(string patientName)
        {
            List<string> studyUIDList = await FindStudy(patientName);
            Dictionary<string, List<string>> seriesDict = new();

            foreach (string studyUID in studyUIDList)
            {
                seriesDict.Add(studyUID, await FindSeries(studyUID));
            }
            return seriesDict;
        }

        public async Task RunCGet(string studyUID, string seriesUID)
        {
            DicomCGetRequest request = new(studyUID, seriesUID);
            Client.OnCStoreRequest += (DicomCStoreRequest req) =>
            {
                Console.WriteLine(DateTime.Now.ToString() + " received");
                SaveImage(req.Dataset);
                return Task.FromResult(new DicomCStoreResponse(req, DicomStatus.Success));
            };

            var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                DicomStorageCategory.Image,
                DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRBigEndian);
            Client.AdditionalPresentationContexts.AddRange(pcs);

            await Client.AddRequestAsync(request);
            await Client.SendAsync();
        }

        private async Task<List<string>> FindStudy(string patientName)
        {
            var request = CreateStudyRequestByPatientName(patientName);
            List<string> studyUIDList = new();
            request.OnResponseReceived += (req, response) =>
              {
                  LogStudyResponse(response);
                  studyUIDList.Add(response.Dataset?.GetSingleValue<string>(DicomTag.StudyInstanceUID));
              };

            await Client.AddRequestAsync(request);
            await Client.SendAsync();
            return studyUIDList;
        }

        private async Task<List<string>> FindSeries(string studyUID)
        {
            var request = CreateSeriesRequestByStudyUID(studyUID);
            List<string> seriesUIDList = new();
            request.OnResponseReceived += (req, response) =>
            {
                LogSeriesResponse(response);
                seriesUIDList.Add(response.Dataset?.GetSingleValue<string>(DicomTag.SeriesInstanceUID));
            };

            await Client.AddRequestAsync(request);
            await Client.SendAsync();
            return seriesUIDList;
        }

        private static DicomCFindRequest CreateStudyRequestByPatientName(string patientName)
        {
            DicomCFindRequest request = new(DicomQueryRetrieveLevel.Study);

            request.Dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, _Encoding);
            request.Dataset.AddOrUpdate(DicomTag.PatientName, "");
            request.Dataset.AddOrUpdate(DicomTag.PatientID, "");
            request.Dataset.AddOrUpdate(DicomTag.ModalitiesInStudy, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyDescription, "");
            request.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);

            return request;
        }

        private static DicomCFindRequest CreateSeriesRequestByStudyUID(string studyInstanceUID)
        {
            DicomCFindRequest request = new(DicomQueryRetrieveLevel.Series);

            request.Dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, _Encoding);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesDescription, "");
            request.Dataset.AddOrUpdate(DicomTag.Modality, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyInstanceUID);

            return request;
        }

        private static void LogStudyResponse(DicomCFindResponse response)
        {
            if (response.Status == DicomStatus.Pending)
            {
                Console.WriteLine($"Patient " +
                    $"{response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)}," +
                    $"{response.Dataset.GetSingleValueOrDefault(DicomTag.ModalitiesInStudy, string.Empty)}" +
                    $"-Study from {response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, new DateTime())}" +
                    $" with UID {response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty)}");
            }
            else if (response.Status == DicomStatus.Success)
            {
                Console.WriteLine(response.Status.ToString());
            }
        }

        private static void LogSeriesResponse(DicomCFindResponse response)
        {
            try
            {
                if (response.Status == DicomStatus.Pending)
                {
                    Console.WriteLine($"Patient " +
                        $"{response.Dataset.GetSingleValue<string>(DicomTag.SeriesDescription)}," +
                        $"{response.Dataset.GetSingleValue<string>(DicomTag.Modality)}," +
                        $"-Study from {response.Dataset.GetSingleValue<int>(DicomTag.NumberOfSeriesRelatedInstances)} instances");
                }
                else if (response.Status == DicomStatus.Success)
                {
                    Console.WriteLine(response.Status.ToString());
                }
            }
            catch (Exception)
            {

            }
        }

        private static void SaveImage(DicomDataset dataset)
        {
            var studyUID = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID).Trim();
            var sopUID = dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID).Trim();
            var path = Path.GetFullPath(_StoragePath);
            path = Path.Combine(path, studyUID);

            if (Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, sopUID) + ".dcm";
            new DicomFile(dataset).Save(path);
        }
    }
}
