import json

from database import MyPACSdatabase
from utils import config_root,get_db_connection

if __name__ == '__main__':
    with open(config_root, 'r') as file:
        db_config = json.load(file)['database']

    db=get_db_connection(db_config)

    db.query_return_none('DROP TABLE IF EXISTS mypacs_test')
    db.query_file_return_none('./sql/create.sql')
