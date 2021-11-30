import json
from pathlib import Path

from progress.bar import Bar
import click

from myDB import MyDB
from utils import generate_record_dict

data_root = r'E:\manifest-7qRRRBGo6029235898952856192\LungCT-Diagnosis'


@click.command()
@click.option('--root', default=data_root, help='DICOM file root')
@click.option('--config', default='./config.json', help='config file path')
def main(root, config):
    with open(config, 'r') as file:
        db_config = json.load(file)['database']

    connect_str = f"mysql+pymysql://{db_config['username']}:{db_config['password']}" \
                  f"@{db_config['host']}:{db_config['port']}/{db_config['name']}"
    db = MyDB(connect_str)
    print(db.get_connection())

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
