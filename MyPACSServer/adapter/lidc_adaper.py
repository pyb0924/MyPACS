from pydicom import Dataset

from adapter.adapter_base import AdapterBase


class LIDCAdapter(AdapterBase):
    def __init__(self, annotation_path: str):
        super().__init__(annotation_path)

    def _parse_annotation(self):
        pass

    def get_overlay(self, dataset: Dataset):
        pass

    def get_pixel(self, dataset: Dataset):
        pass
