import json
import logging.config
from functools import partial
import traceback

from pydicom import dcmread
from pydicom.dataset import Dataset
from pynetdicom import AE, StoragePresentationContexts, evt
from pynetdicom import sop_class

from utils import get_db_connection


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
        self.db = get_db_connection(config['database'])

        # init logger
        logging.config.dictConfig(config['logger'])
        self.logger = logging.getLogger('MyPACSLogger')

    def run(self):
        self.logger.debug(f'Server running at port {self.port}, AET title: {str(self.ae_title, encoding="utf-8")}')
        self.start_server(('localhost', self.port), ae_title=self.ae_title, evt_handlers=self.handlers)

    def handle_find_wrapper(self):
        return partial(self.handle_find, server=self)

    def handle_get_wrapper(self):
        return partial(self.handle_find, server=self)

    @staticmethod
    def handle_find(event, server):
        """ Handle C-Find request by PatientName"""

        req_dataset = event.identifier

        if 'QueryRetrieveLevel' not in req_dataset or req_dataset.QueryRetrieveLevel != 'PATIENT':
            # Failure
            server.logger.error("C-Find: Invalid QueryRetrieveLevel")
            yield 0xC300, None
            return

        if 'PatientName' not in req_dataset or req_dataset.PatientName in ['*', '', '?']:
            server.logger.error("C-Find: Invalid PatientName")
            yield 0xC300, None
            return

        server.logger.debug(f'C-Find: Check request successfully! PatientName={req_dataset.PatientName}')

        try:
            find_rows = server.db.query_file('./sql/select_by_patient_name.sql', fetchall=True,
                                             patient_name=req_dataset.PatientName)
        except Exception:
            server.logger.exception(f"C-Find: Exception occured:{traceback.format_exc()}")
        else:
            rows_dict_list = find_rows.as_dict()
            server.logger.debug(f'C-Find: found {len(rows_dict_list)} results in database')

            for row in rows_dict_list:
                # Check if C-CANCEL has been received
                if event.is_cancelled:
                    yield 0xFE00, None
                    server.logger.debug(f'C-Find success. Found {len(rows_dict_list)} DICOM instances')
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
        """Handle C-GET request by StudyInstanceUID & SeriesInstanceUID."""
        # TODO add supprt for image processing
        req_dataset = event.identifier

        if 'StudyInstanceUID' not in req_dataset or 'SeriesInstanceUID' not in req_dataset:
            server.logger.error('C-GET: Invalid request')
            yield 0xC400, None
            return
        try:
            get_rows = server.db.query_file('./sql/select_by_id.sql', fetchall=True,
                                            study_instance_uid=req_dataset.StudyInstanceUID,
                                            series_instance_uid=req_dataset.SeriesInstanceUID)
        except Exception:
            server.logger.exception(f"C-GET: Exception occured:{traceback.format_exc()}")
        else:
            rows_dict_list = get_rows.as_dict()
            server.logger.debug(f'C-GET: found {len(rows_dict_list)} result in database')

            # Yield the total number of C-STORE sub-operations required
            yield len(rows_dict_list)

            # Yield the matching instances
            for row in rows_dict_list:
                # Check if C-CANCEL has been received
                if event.is_cancelled:
                    yield 0xFE00, None
                    server.logger.debug(f'C-GET success. Retrieved {len(rows_dict_list)} DICOM instances')
                    return

                res_dataset = dcmread(row['local_file_path'])

                # Pending
                yield 0xFF00, res_dataset
