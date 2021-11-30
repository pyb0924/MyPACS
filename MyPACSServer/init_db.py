import json
from myDB import MyDB

if __name__ == '__main__':
    with open('./config.json', 'r') as file:
        db_config = json.load(file)['database']

    connect_str = f"mysql+pymysql://{db_config['username']}:{db_config['password']}" \
                  f"@{db_config['host']}:{db_config['port']}/{db_config['name']}"
    db = MyDB(connect_str)
    print(db.get_connection())

    db.query_return_none('DROP TABLE IF EXISTS mypacs_test')
    db.query_file_return_none('./sql/create.sql')
