from abc import ABC, abstractmethod
from pydicom import Dataset


class AdapterBase(ABC):
    def __init__(self, annotation_path: str):
        self.annotation = annotation_path

    @abstractmethod
    def _parse_annotation(self):
        pass

    @abstractmethod
    def get_overlay(self, dataset: Dataset):
        pass

    @abstractmethod
    def get_pixel(self, dataset: Dataset):
        pass

    def __call__(self, dataset: Dataset, use_overlay=True, **kwargs):
        if use_overlay:
            return self.get_overlay(dataset)
        else:
            return self.get_pixel(dataset)
