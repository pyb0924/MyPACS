import records
from sqlalchemy.exc import ResourceClosedError


class MyPACSdatabase(records.Database):
    def __init__(self, db_url=None, **kwargs):
        super().__init__(db_url, **kwargs)

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
