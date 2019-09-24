# -*- coding: utf-8 -*-
import serial
import datetime
import time

LOG = True

a = serial.Serial('COM5',115200)
a.write(b'\x01')
if LOG: datafile = open(datetime.datetime.now().strftime('acc4k-%Y%m%d-%H%M%S')+'.txt','w')

cnt = 0
while True:
    data = a.read(14)
    # print(type(data))
    num = []
    for i in range(7):
        num.append(int((data[i*2]<<8) | data[i*2+1]))
        if num[i] > 2**15: num[i] -= 2**16
    acc = num[0:3]
    ther = num[3:4]
    gyro = num[4:7]

    if LOG: datafile.write(str(time.time()) + ' ' + str(acc[0]) + ' ' + str(acc[1]) + ' ' + str(acc[2]) + '\n')
    if cnt % 100 == 0:
        print(acc, ther, gyro)
    cnt += 1

'''
连线
模块 -> 开发板
VCC     3.3
GND     G
SCLK    A5
SDI     A7
SDO     A6
INT     A1
NCS     A4

读入数据14个byte：加速度(6), 温度(2), 陀螺仪(6)
'''
