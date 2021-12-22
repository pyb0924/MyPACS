from pydicom import Dataset

from .adapter_base import AdapterBase


class LIDCAdapter(AdapterBase):

    @classmethod
    def _parse_annotation(cls, annotation_path: str):
        return None

    @classmethod
    def _get_overlay(cls, dataset: Dataset, annotation) -> Dataset:
        return dataset

    @classmethod
    def _get_pixel(cls, dataset: Dataset, annotation) -> Dataset:
        dataset.PixelData = annotation
        return dataset
