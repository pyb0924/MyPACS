# MyPACS

## Prerequisite

- C\#前端
  - WPF
  - .NET 5.0
  - [fo-dicom 4](https://fo-dicom.github.io/dev/v4/)

- Python后端
  - Python 3.9
  - [requirements.txt](./MyPACSServer/requirements.txt)
- 图像处理
  - PyTorch




## Overview

- 一个完整的基于DICOM协议的PACS系统。
- 实现功能：
  - 将产生的DICOM文件存储到远端Server并将其序列写入数据库。
  - 打开本地DICOM文件和文件夹并显示DICOM序列。
  - DICOM传输，从远端Server通过DICOM Q-R获取DICOM图像。
  - 对DICOM图像进行一些简单的图像处理（在前端处理）和进行图像分割（向后端请求）。
  



## Submodules

包含以下几个模块：

- [StudySCU](./studySCU)：一个DICOM C-STORE SCU，用于模拟产生图像的检查设备端。
- [MyPACSViewer](./MyPACSViewer)：一个集成了ViewerSCU（一个DICOM Q-R SCU）的前端UI界面，模拟的是客户端。
- [MyPACSServer](./MyPACSServer)：一个DICOM Q-R/C-STORE SCP，并连接了一个本地用于存储DICOM数据索引信息和文件路径的MySQL数据库和一个DICOM图像处理模块，模拟一个DICOM服务器工作站。
- [图像处理模块](./MyPACSServer/processin)

<img src="E:\BM425\report\docs\MyPACS\structure.png"  />

- 整个系统的工作流程如下：
  1. StudySCU将DICOM图片使用C-STORE传输到MyPACSServer，并将其索引信息和文件路径写入数据库。
  2. 用户在使用MyPACSViewer时，可以打开本地文件浏览，也可以通过DICOM Q-R机制，从MyPACSServer获取DICOM文件。
  3. 用户可以对图像进行简单的处理，这一部分的处理在MyPACSViewer内部进行。
  4. 对于较复杂的图像处理（如图像分割），通过在C-GET Request中插入特定的Tag信息，使MyPACSServer返回相应的处理结果。



### [StudySCU](./StudySCU)

- 一个基于C\#命令行应用的DICOM C-STORE SCU
- DICOM协议使用fo-dicom库。

### [MyPACSViewer](./MyPACSViewer)

- 一个基于C\#语言WPF（Windows Presentation Foundation）开发的DICOM Viewer。

- DICOM协议使用fo-dicom库。

  ![image-20211119112618344](C:/Users/pyb0924/AppData/Roaming/Typora/typora-user-images/image-20211119112618344.png)

- 使用MVVM设计模式（Model-View-View Model）

  

### [MyPACSServer](./MyPACSServer)

- 一个基于Python 3.9开发的DICOM Server。
- DICOM协议使用pydicom和pynetdicom库。
- 数据库使用MySQL，并使用pymysql和records库进行连接和处理。
- 使用Python内置的logging模块在控制台输出日志并写入文件。



### [图像处理模块](./MyPACSServer/processing)
