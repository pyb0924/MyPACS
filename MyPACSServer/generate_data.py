import json
from pathlib import Path

from pydicom import dcmread
import click
from progress.bar import Bar

from utils import config_root
from database import MyPACSdatabase

columns = ['sop_instance_uid', 'patient_name', 'patient_id', 'study_instance_uid', 'modality', 'body_part_examined',
           'series_description', 'series_instance_uid', 'local_file_path']


def create_db(db):
    db.query_return_none('DROP TABLE IF EXISTS mypacs_test')
    db.query_file_return_none('./sql/create.sql')


def insert_data_to_db(db, roots):
    for root in roots:
        dicom_path_list = list(Path(root).rglob('*.dcm'))

        record_list = []
        with Bar(f'Processing DICOM Files at root[{root}]:', max=len(dicom_path_list)) as bar:
            for path in dicom_path_list:
                record_list.append(generate_record_dict(str(path)))
                bar.next()

        db.bulk_query_file('./sql/insert.sql', record_list)

    print('Insert all DICOM files to DB!')


def generate_record_dict(path: str):
    dataset = dcmread(path)
    values = [dataset.SOPInstanceUID, dataset.PatientName, dataset.PatientID, dataset.StudyInstanceUID,
              dataset.Modality, dataset.BodyPartExamined, dataset.SeriesDescription, dataset.SeriesInstanceUID, path]
    return dict(zip(columns, values))


@click.command()
@click.option('--root', '-r', help='DICOM file root', multiple=True)
@click.option('--config', '-c', default=config_root, help='config file path')
def main(root, config):
    with open(config, 'r') as file:
        db_config = json.load(file)['database']

    db = MyPACSdatabase(db_config)
    create_db(db)
    insert_data_to_db(db, root)


if __name__ == '__main__':
    main()
