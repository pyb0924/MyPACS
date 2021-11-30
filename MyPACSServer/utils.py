from pydicom import dcmread, Dataset

columns = ['sop_instance_uid', 'patient_name', 'patient_id', 'study_id', 'study_instance_uid', 'series_description',
           'series_instance_uid', 'local_file_path']


def generate_record_dict(path: str):
    dataset = dcmread(path)
    values = [dataset.SOPInstanceUID, dataset.PatientName, dataset.PatientID, dataset.StudyID, dataset.StudyInstanceUID,
              dataset.SeriesDescription, dataset.SeriesInstanceUID, path]
    return dict(zip(columns, values))
