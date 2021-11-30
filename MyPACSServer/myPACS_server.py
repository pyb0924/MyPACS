import json
import logging.config
from pathlib import Path
import os
from functools import partialmethod, partial
from pydicom import dcmread
from pydicom.dataset import Dataset

from pynetdicom import AE, StoragePresentationContexts, evt
from pynetdicom import sop_class
from pynetdicom.status import STATUS_PENDING

import records


class MyPACSServer(AE):
    def __init__(self, config_file):
        super().__init__()

        # read config file
        with open(config_file, 'r') as file:
            config = json.load(file)

        # init scp
        self.port = config['scp']['port']
        self.ae_title = config['scp']['ae_title']

        self.supported_contexts = StoragePresentationContexts
        for cx in self.supported_contexts:
            cx.scp_role = True
            cx.scu_role = False

        self.add_supported_context(sop_class.PatientRootQueryRetrieveInformationModelFind)
        self.add_supported_context(sop_class.PatientRootQueryRetrieveInformationModelGet)
        self.handlers = [
            (evt.EVT_C_GET, self.handle_find_wrapper()),
            (evt.EVT_C_FIND, self.handle_get_wrapper())
        ]

        # init database
        config_db = config['database']
        self.connect_str = f"mysql+pymysql://{config_db['username']}:{config_db['password']}" \
                           f"@{config_db['host']}:{config_db['port']}/{config_db['name']}"
        self.db = records.Database(self.connect_str)

        # init logger
        logging.config.dictConfig(config['logger'])
        self.logger = logging.getLogger('MyPACSLogger')

    def handle_find_wrapper(self):
        return partial(self.handle_find, server=self)

    def handle_get_wrapper(self):
        return partial(self.handle_find, server=self)

    @staticmethod
    def handle_find(event, server):
        """
        Handle C-Find by PatientName
        """

        req_dataset = event.identifier

        if 'QueryRetrieveLevel' not in req_dataset:
            # Failure
            yield 0xC000, None
            return

        # Import stored SOP Instances
        instances = []
        if req_dataset.PatientName not in ['*', '', '?']:
            rows = server.db.query('./sql/select_by_patient_name.sql', fetchall=True,
                                   patient_name=req_dataset.PatientName)
        else:
            server.logger.error("Invalid PatientName")
            yield 0xC000,None
            return

        rows_dict_list = rows.as_dict()

        for row in rows_dict_list:
            # Check if C-CANCEL has been received
            if event.is_cancelled:
                yield 0xFE00, None
                return

            res_dataset = Dataset()
            res_dataset.PatientName = row['patient_name']
            res_dataset.StudyInstanceUID = row['study_instance_uid']
            res_dataset.SeriesDescription = row['series_description']
            res_dataset.SeriesInstanceUID = row['series_instance_uid']

            # Pending
            yield 0xFF00, res_dataset

    @staticmethod
    def handle_get(event, server):
        """Handle a C-GET request event."""
        req_dataset = event.identifier
        if 'QueryRetrieveLevel' not in req_dataset:
            # Failure
            yield 0xC000, None
            return

        # Import stored SOP Instances
        instances = []
        matching = []
        fdir = '/path/to/directory'
        for fpath in os.listdir(fdir):
            instances.append(dcmread(os.path.join(fdir, fpath)))

        if req_dataset.QueryRetrieveLevel == 'PATIENT':
            if 'PatientID' in req_dataset:
                matching = [
                    inst for inst in instances if inst.PatientID == req_dataset.PatientID
                ]

            # Skip the other possible attributes...

        # Skip the other QR levels...

        # Yield the total number of C-STORE sub-operations required
        yield len(instances)

        # Yield the matching instances
        for instance in matching:
            # Check if C-CANCEL has been received
            if event.is_cancelled:
                yield 0xFE00, None
                return

            # Pending
            yield 0xFF00, instance

    def run(self):
        self.logger.debug(f'Server running at port {self.port}')
        self.start_server(('localhost', self.port), ae_title=self.ae_title, evt_handlers=self.handlers)
