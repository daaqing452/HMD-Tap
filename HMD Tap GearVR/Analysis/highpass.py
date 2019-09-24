import matplotlib.pyplot as plt
import numpy as np
from scipy import signal
from utils import *

fs=100

names = ['guyizheng', 'huyuan', 'liguohao', 'mengqi', 'shijiaxin', 'sunke', 'wangruolin', 'weixiaoying', 'xiawu', 'xiexiaohui', 'yanyukang']

a = read(filename='./data/' + names[0] + '/wander.txt')
g = read(filename='./data/' + names[0] + '/left.txt')
#a = a[:, 880:910]

af = time2freq(a, fs=fs)
gf = time2freq(g, fs=fs)
'''n = a.shape[1]
print('n:', n)
l = int(20.0/fs*n)
af[1:, :l] = np.zeros((6,l))
af[1:, n-l:] = np.zeros((6,l))
b = np.fft.ifft(af, axis=1)'''

coeff_b, coeff_a = signal.butter(3, 0.4, 'highpass')
ab = signal.filtfilt(coeff_b, coeff_a, a)
gb = signal.filtfilt(coeff_b, coeff_a, g)
#print(b[1,0000:2000].max())
#print(b[1,2000:3000].max())

showb = 1
for i in range(3):
	plt.subplot(9, 1, i+1)
	plt.plot(a[i+showb])
for i in range(3):
	plt.subplot(9, 1, i+4)
	plt.plot(af[0], np.abs(af[i+showb]))
for i in range(3):
	plt.subplot(9, 1, i+7)
	plt.plot(ab[i+showb])
#plt.show()

fig = plt.figure()
showb = 1
for i in range(3):
	plt.subplot(9, 1, i+1)
	plt.plot(g[i+showb])
for i in range(3):
	plt.subplot(9, 1, i+4)
	plt.plot(gf[0], np.abs(gf[i+showb]))
for i in range(3):
	plt.subplot(9, 1, i+7)
	plt.plot(gb[i+showb])
plt.show()