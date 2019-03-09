# MmPort

MmPort旨在简化工业控制领域上位机通信模块的工作，目前提供传统的串口通信和modbus通信方式。

## 文件说明
```
MmConfiguration.cs  --> 配置类
MmDataReceiveHander.cs  --> 串口接收事件，集成该类，重写数据接收方法，即可自定义处理数据。
MmModbus.cs  --> modbus组件，提供modbus协议的多种读写功能。
MmPortManager.cs  --> 串口对象管理器，通过构造函数传入配置类配置，通过该类的实例方法DefaultMmPortManager获取串口对象。 
MmSerialPort.cs --> 普通串口协议组件，支持自定义协议，包头，数据长度，校验，超时，重发等。
MmUtils.cs --> 工具类，提供常用的数据格式转换，校验方法。
```
## 用法

#### 添加引用
从文件中复制MmPort.dll文件，在项目工程添加改dll的引用。
```
Useing MmPort;

```
#### 获取串口对象

```
MmModbus  mmModbus =(MmModbus)new MmPortManager().DefaultMmPortManager(COMMUNICATION_TYPE.Modbus, new MmConfiguration("COM3",9600),new myDataReceiveHander());

```

#### 使用
根据得到的串口对象选择合适的方法进行数据收发即可。。


## 注意
其中COMMUNICATION_TYPE枚举对应两个值，选择Modbus或者SerialPort。



# ***水平有限，如有错误或不足，请多多指教，持续更新中。***