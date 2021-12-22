from pathlib import Path
from pydicom import dcmread, dcmwrite
from adapter import LIDCAdapter

if __name__ == '__main__':
    seriesFolder = Path(r'E:\LIDC\LIDC-IDRI-0001\1.3.6.1.4.1.14519.5.2.1.6279.6001.298806137288633453246975630178')
    xml_path = next(seriesFolder.rglob('*.xml'))
    adapter_dict = {"LIDC": LIDCAdapter}
    for dicom_file in seriesFolder.rglob('*.dcm'):
        dataset = dcmread(dicom_file)
        sopuid = dataset.SOPInstanceUID
        overlay_dataset = LIDCAdapter.get_annotation(dataset, str(xml_path))
        dcmwrite('./overlay/' + sopuid + '.dcm', overlay_dataset)
