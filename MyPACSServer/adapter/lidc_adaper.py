import pydicom
from pydicom import Dataset
from lxml import etree
import numpy as np
from pydicom.pixel_data_handlers.numpy_handler import pack_bits
from .adapter_base import AdapterBase



class LIDCAdapter(AdapterBase):

    @classmethod
    def _parse_annotation(cls, dataset: Dataset, annotation_path: str):
        # dataset info
        row = dataset.pixel_array.shape[0]
        line = dataset.pixel_array.shape[1]
        print('datasetSize:', line, row)

        # get SOP list
        try:
            tree = etree.parse(annotation_path, etree.XMLParser())
            root = tree.getroot()
            reading_session = root.find(path='readingSession', namespaces=root.nsmap)
            SOP_UID_list = reading_session.findall(path='unblindedReadNodule/roi/imageSOP_UID', namespaces=root.nsmap)
            print('len(SOP_UID_list):', len(SOP_UID_list))
            print('SOP_UID_list:', SOP_UID_list)
            # reading session 1 下总计11个SOPUID（check）

            for SOP_UID in SOP_UID_list:
                # if SOP_UID.text=='1.3.6.1.4.1.14519.5.2.1.6279.6001.499837844441581448374672853475':
                if SOP_UID.text == dataset.SOPInstanceUID:
                    # 这里SOP_UID_list里面的elem不是这个格式，是<Element {http://www.nih.gov}imageSOP_UID at 0x7f8464b541c8>这样子的格式,改成.text输出成立
                    # 选取上一层节点roi
                    roi = SOP_UID.getparent()
                    print('roi.tag:', roi.tag)

                    # 判断roi上一层是不是unblindedReadNodule
                    if roi.getparent().tag == '{http://www.nih.gov}unblindedReadNodule':
                        # check the tag
                        print('roi.getparent().tag:', roi.getparent().tag)
                        unblindedReadNodule = roi.getparent()

                        # check characteristics
                        # characteristics=unblindedReadNodule.findall(
                        #    'characteristics', namespaces=root.nsmap)
                        # print(len(characteristics))

                        # 判断同一层是不是有节点characteristic
                        if unblindedReadNodule.findall('characteristics', namespaces=root.nsmap):
                            # 获取当前roi下的<edgeMap>对应的<xCoord><yCoord>位置像素设置为1，其余为0
                            edgemap_list = roi.findall('edgeMap', namespaces=root.nsmap)
                            print('len(edgemap_list):', len(edgemap_list))
                            # 确定该roi下对应的edgemap总共117个（113）

                            # 创建一个大小为row/line的二维数组，全部为0
                            mask_array = np.zeros([row, line])
                            # print(mask_array)

                            # 获取edgemap下对应的<xCoord><yCoord>,并设置当前数组位置为1
                            for edgemap in edgemap_list:
                                xCoord_edgemap = edgemap.find('xCoord', namespaces=root.nsmap).text
                                yCoord_edgemap = edgemap.find('yCoord', namespaces=root.nsmap).text
                                # print('x/yCoord_edgemap:', xCoord_edgemap,yCoord_edgemap )
                                mask_array[int(xCoord_edgemap), int(yCoord_edgemap)] = 1
                return mask_array
        except Exception as ex:
            print(ex)

        return None

                        # print(mask_array)
        # check the array pic via matplotlib
        #plt.imshow(mask_array)
        #plt.show()


    @classmethod
    def _get_overlay(cls, dataset: Dataset, annotation_path:str) -> Dataset:
        # deal with the overlay: https://www.docin.com/p-1628132723.html&key=DIC怎么治
        # create the overlay
        annotation=cls._parse_annotation(dataset,annotation_path)
        # from pydicom.pixel_data_handlers.numpy_handler import pack_bits
        packed_bytes = pack_bits(annotation)
        # dataset[0x6000, 0x3000].value = packed_bytes

        elem = pydicom.DataElement(0x60003000, VR='US', value=packed_bytes)
        dataset.add(elem)
        # print(dataset[0x60003000].value)
        return dataset

    @classmethod
    def _get_pixel(cls, dataset: Dataset, annotation_path:str) -> Dataset:
        annotation=cls._parse_annotation(dataset,annotation_path)
        dataset.PixelData=annotation.tobytes()
        return dataset
