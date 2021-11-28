from pydicom import dcmread
from pydicom.dataset import Dataset
import os

from pynetdicom import AE, StoragePresentationContexts, evt
from pynetdicom import sop_class


class MyPACSServer(AE):
    def __init__(self, port):
        super().__init__()
        self.port = port

        self.ae = AE()
        self.ae.supported_contexts = StoragePresentationContexts
        for cx in self.ae.supported_contexts:
            cx.scp_role = True
            cx.scu_role = False

        self.ae.add_supported_context(sop_class.PatientRootQueryRetrieveInformationModelFind)
        self.ae.add_supported_context(sop_class.PatientRootQueryRetrieveInformationModelGet)
        self.handlers = [
            (evt.EVT_C_GET, MyPACSServer.handle_get),
            (evt.EVT_C_FIND,MyPACSServer.handle_find)
        ]

    @staticmethod
    def handle_find(event):
        """Handle a C-FIND request event."""
        ds = event.identifier

        # Import stored SOP Instances
        instances = []
        fdir = '/path/to/directory'
        for fpath in os.listdir(fdir):
            instances.append(dcmread(os.path.join(fdir, fpath)))

        if 'QueryRetrieveLevel' not in ds:
            # Failure
            yield 0xC000, None
            return

        if ds.QueryRetrieveLevel == 'PATIENT':
            if 'PatientName' in ds:
                if ds.PatientName not in ['*', '', '?']:
                    matching = [
                        inst for inst in instances if inst.PatientName == ds.PatientName
                    ]

                # Skip the other possibile values...

            # Skip the other possible attributes...

        # Skip the other QR levels...

        for instance in matching:
            # Check if C-CANCEL has been received
            if event.is_cancelled:
                yield 0xFE00, None
                return

            identifier = Dataset()
            identifier.PatientName = instance.PatientName
            identifier.QueryRetrieveLevel = ds.QueryRetrieveLevel

            # Pending
            yield 0xFF00, identifier

    @staticmethod
    def handle_get(event):
        """Handle a C-GET request event."""
        ds = event.identifier
        if 'QueryRetrieveLevel' not in ds:
            # Failure
            yield 0xC000, None
            return

        # Import stored SOP Instances
        instances = []
        matching = []
        fdir = '/path/to/directory'
        for fpath in os.listdir(fdir):
            instances.append(dcmread(os.path.join(fdir, fpath)))

        if ds.QueryRetrieveLevel == 'PATIENT':
            if 'PatientID' in ds:
                matching = [
                    inst for inst in instances if inst.PatientID == ds.PatientID
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
        self.ae.start_server(('', self.port), evt_handlers=self.handlers)
