import records
from sqlalchemy.exc import ResourceClosedError


class MyPACSdatabase(records.Database):
    def __init__(self, db_config):
        connect_str = f"mysql+pymysql://{db_config['username']}:{db_config['password']}" \
                      f"@{db_config['host']}:{db_config['port']}/{db_config['name']}"
        super().__init__(connect_str)

    def query_return_none(self, query: str, **kwargs):
        try:
            self.query(query, **kwargs)
        except ResourceClosedError:
            pass

    def query_file_return_none(self, query_file: str, **kwargs):
        try:
            self.query_file(query_file, fetchall=True, **kwargs)
        except ResourceClosedError:
            pass
