import json
from pathlib import Path

from progress.bar import Bar
import click

from database import MyPACSdatabase
from utils import generate_record_dict, config_root, get_db_connection


@click.command()
@click.option('--root', help='DICOM file root')
@click.option('--config', default=config_root, help='config file path')
def main(root, config):
    with open(config, 'r') as file:
        db_config = json.load(file)['database']

    db = get_db_connection(db_config)

    dicom_path_list = list(Path(root).rglob('*.dcm'))

    record_list = []
    with Bar('Processing DICOM Files:', max=len(dicom_path_list)) as bar:
        for path in dicom_path_list:
            record_list.append(generate_record_dict(str(path)))
            bar.next()

    db.bulk_query_file('./sql/insert.sql', record_list)

    print('Insert all DICOM files to DB!')


if __name__ == '__main__':
    main()
