import pydicom
from pydicom import Dataset
from lxml import etree
import numpy as np
from pydicom.pixel_data_handlers.numpy_handler import pack_bits
from .adapter_base import AdapterBase


class LIDCAdapter(AdapterBase):

    @classmethod
    def _parse_annotation(cls, dataset: Dataset, annotation_path: str):
        tree = etree.parse(annotation_path, etree.XMLParser())
        root = tree.getroot()
        reading_session = root.find(path='readingSession', namespaces=root.nsmap)
        roi_list = reading_session.findall(
            path=f'unblindedReadNodule[characteristics]/roi[imageSOP_UID="{dataset.SOPInstanceUID}"]',
            namespaces=root.nsmap)
        # print(dataset.SOPInstanceUID)
        annotation_array = np.zeros([dataset.Rows, dataset.Columns])
        for roi in roi_list:
            edgemap_list = roi.findall('edgeMap', namespaces=root.nsmap)
            for edgemap in edgemap_list:
                xCoord_edgemap = edgemap.find('xCoord', namespaces=root.nsmap).text
                yCoord_edgemap = edgemap.find('yCoord', namespaces=root.nsmap).text
                annotation_array[int(yCoord_edgemap), int(xCoord_edgemap)] = 1
        return annotation_array

    @classmethod
    def _get_overlay(cls, dataset: Dataset, annotation_path: str) -> Dataset:
        annotation = cls._parse_annotation(dataset, annotation_path)
        elem_overlay_type = pydicom.DataElement(0x60000040, VR='CS', value='GRAPHICS ')
        dataset.add(elem_overlay_type)
        elem_overlay_rows = pydicom.DataElement(0x60000010, VR='US', value=dataset.Rows)
        dataset.add(elem_overlay_rows)
        elem_overlay_columns = pydicom.DataElement(0x60000011, VR='US', value=dataset.Columns)
        dataset.add(elem_overlay_columns)
        elem_overlay_bit_allocated = pydicom.DataElement(0x60000100, VR='US', value=1)
        dataset.add(elem_overlay_bit_allocated)
        elem_overlay_bit_position = pydicom.DataElement(0x60000102, VR='US', value=0)
        dataset.add(elem_overlay_bit_position)
        elem_overlay_origin = pydicom.DataElement(0x60000050, VR='SS', value=[1, 1])
        dataset.add(elem_overlay_origin)
        elem_overlay_data = pydicom.DataElement(0x60003000, VR='OW', value=pack_bits(annotation))
        dataset.add(elem_overlay_data)
        return dataset

    @classmethod
    def _get_pixel(cls, dataset: Dataset, annotation_path: str) -> Dataset:
        annotation = cls._parse_annotation(dataset, annotation_path)
        dataset.PixelData = annotation.tobytes()
        return dataset
