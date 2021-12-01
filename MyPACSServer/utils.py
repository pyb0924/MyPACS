from pydicom import dcmread
from database import MyPACSdatabase

columns = ['sop_instance_uid', 'patient_name', 'patient_id', 'study_id', 'study_instance_uid', 'series_description',
           'series_instance_uid', 'local_file_path']

config_root = r'./config.json'


def get_db_connection(db_config):
    connect_str = f"mysql+pymysql://{db_config['username']}:{db_config['password']}" \
                  f"@{db_config['host']}:{db_config['port']}/{db_config['name']}"
    db = MyPACSdatabase(connect_str)
    return db


def generate_record_dict(path: str):
    dataset = dcmread(path)
    values = [dataset.SOPInstanceUID, dataset.PatientName, dataset.PatientID, dataset.StudyID, dataset.StudyInstanceUID,
              dataset.SeriesDescription, dataset.SeriesInstanceUID, path]
    return dict(zip(columns, values))
