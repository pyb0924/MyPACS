import pytest
import json
from database import MyPACSdatabase


@pytest.fixture
def database():
    with open('./config.json', 'r') as file:
        db_config = json.load(file)['database']
    return MyPACSdatabase(db_config)

