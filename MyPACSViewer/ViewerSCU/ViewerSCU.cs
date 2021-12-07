using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using FellowOakDicom;

using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

namespace ViewerSCU
{
    public class ViewerSCU
    {
        private string StoragePath { get; }
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

            var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                DicomStorageCategory.Image,
                DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRBigEndian);
            Client.AdditionalPresentationContexts.AddRange(pcs);
        }

        public ViewerSCU(string host, int port, string serverAET, string aet,string storagePath)
        {
            Host = host;
            Port = port;
            ServerAET = serverAET;
            Aet = aet;
            StoragePath = storagePath;
            Client = DicomClientFactory.Create(Host, Port, false, Aet, ServerAET);
            Client.NegotiateAsyncOps();

            var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                DicomStorageCategory.Image,
                DicomTransferSyntax.ExplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRLittleEndian,
                DicomTransferSyntax.ImplicitVRBigEndian);
            Client.AdditionalPresentationContexts.AddRange(pcs);
        }

        public async Task<List<DicomDataset>> RunCFind(string patient, bool use_id = false)
        {
            DicomCFindRequest request = use_id ? CreateFindRequestByPatientID(patient) : CreateFindRequestByPatientName(patient);
            List<DicomDataset> resDatasetList = new();
            request.OnResponseReceived += (req, response) =>
            {
                LogFindResponse(response);
                resDatasetList.Add(response.Dataset);
            };

            await Client.AddRequestAsync(request);
            await Client.SendAsync();
            return resDatasetList;
        }

        public async Task RunCGet(string studyInstanceUID, string seriesInstanceUID)
        {
            DicomCGetRequest request = new(studyInstanceUID, seriesInstanceUID);
            Client.OnCStoreRequest += (DicomCStoreRequest req) =>
            {
                Console.WriteLine(DateTime.Now.ToString() + $"{req.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID)} received");
                SaveImage(req.Dataset);
                return Task.FromResult(new DicomCStoreResponse(req, DicomStatus.Success));
            };
            await Client.AddRequestAsync(request);
            await Client.SendAsync();
        }

        private static DicomCFindRequest CreateFindRequestByPatientName(string patientName)
        {
            DicomCFindRequest request = new(DicomQueryRetrieveLevel.Patient);

            request.Dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, _Encoding);
            request.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.Modality, "");
            request.Dataset.AddOrUpdate(DicomTag.BodyPartExamined, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesDescription, "");

            return request;
        }

        private static DicomCFindRequest CreateFindRequestByPatientID(string patientID)
        {
            DicomCFindRequest request = new(DicomQueryRetrieveLevel.Patient);

            request.Dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, _Encoding);
            request.Dataset.AddOrUpdate(DicomTag.PatientID, patientID);
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.Modality, "");
            request.Dataset.AddOrUpdate(DicomTag.BodyPartExamined, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesDescription, "");

            return request;
        }

        private static void LogFindResponse(DicomCFindResponse response)
        {
            if (response.Status == DicomStatus.Pending)
            {
                Console.WriteLine($"Patient " +
                    $"{response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)}," +
                    $" from Study {response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty)}" +
                    $"{ response.Dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty)}," +
                    $" in Series {response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty)}" +
                    $"{response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, string.Empty)},");
            }
            else if (response.Status == DicomStatus.Success)
            {
                Console.WriteLine(response.Status.ToString());
            }
        }

        private void SaveImage(DicomDataset dataset)
        {
            var studyUID = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID).Trim();
            var seriesUID = dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID).Trim();
            var sopUID = dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID).Trim();
            var path = Path.GetFullPath(StoragePath);

            path = Path.Combine(path, studyUID);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, seriesUID);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, sopUID) + ".dcm";
            new DicomFile(dataset).Save(path);
        }
    }
}
