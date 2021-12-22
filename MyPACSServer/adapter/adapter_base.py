from abc import ABC, abstractmethod
from pydicom import Dataset


class AdapterBase(ABC):

    @classmethod
    @abstractmethod
    def _parse_annotation(cls, annotation_path: str):
        pass

    @classmethod
    @abstractmethod
    def _get_overlay(cls, dataset: Dataset, annotation) -> Dataset:
        pass

    @classmethod
    @abstractmethod
    def _get_pixel(cls, dataset: Dataset, annotation) -> Dataset:
        pass

    @classmethod
    def get_annotation(cls, dataset: Dataset, annotation_path: str, use_overlay=True, **kwargs) -> Dataset:
        annotation = cls._parse_annotation(annotation_path)
        if use_overlay:
            return cls._get_overlay(dataset, annotation)
        else:
            return cls._get_pixel(dataset, annotation)
