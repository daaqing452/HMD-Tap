import matplotlib.pyplot as plt
import numpy as np
from scipy import signal
from sklearn import svm
from utils import *
import pickle
import sys
import time

#names = ['guyizheng','huyuan', 'liguohao', 'luyiqin', 'mengqi', 'shijiaxin', 'sunke', 'wangruolin', 'weixiaoying', 'xiawu', 'xiexiaohui', 'yanyukang']
names = ['guyizheng','huyuan', 'liguohao', 'luyiqin', 'mengqi', 'shijiaxin', 'sunke', 'weixiaoying', 'xiawu', 'xiexiaohui', 'yangzhican', 'yanyukang']

sam = 800
d_sign = []
d_idle = []
d_wand = []
rt = [0.5, 0.1, 0.4]
rtd = [int(sam * rt[0] / 8), int(sam * rt[1] / 8), int(sam * rt[2])]
w = 30

if len(sys.argv) >= 2:
	procedure = sys.argv[1]
else:
	procedure = 'classify'

# sample
if procedure == 'sample':
	for name in names:
		a, b = pickle.load(open('data/' + name + '.data', 'rb'))
		d = read(filename='data/' + name + '/wander.txt')

		for i in range(8):
			ai, bi = a[i], b[i]
			n = ai.shape[1]
			ci = np.zeros(n)
			for l,r in bi:
				ci[l:r] = np.ones((r-l))

			for j in range(rtd[0]):
				while True:
					x = np.random.randint(0, n-w)
					if ci[x:x+w].sum() > w*0.7:
						d_sign.append(ai[:, x:x+w])
						break

			for j in range(rtd[1]):
				while True:
					x = np.random.randint(0, n-w)
					if ci[x:x+w].sum() < 5:
						d_idle.append(ai[:, x:x+w])
						break

		n = d.shape[1]
		for j in range(rtd[2]):
			x = np.random.randint(0, n-w)
			d_wand.append(d[:, x:x+w])

	print(len(d_sign), d_sign[0].shape)
	print(len(d_idle), d_idle[0].shape)
	print(len(d_wand), d_wand[0].shape)

	pickle.dump((d_sign, d_idle, d_wand), open('data/_detect_sample.data', 'wb'))

# show detail
if procedure == 'show':
	d_sign, d_idle, d_wand = pickle.load(open('data/_detect_sample.data', 'rb'))
	ylim = 0.5
	ylim2 = 1.0
	while True:
		x = np.random.randint(rtd[0] * 8 * 12)
		for i in range(6):
			plt.subplot(6, 6, i+1)
			plt.ylim((-ylim, ylim))
			plt.plot(d_sign[x][i+1])
			plt.subplot(6, 6, i+7)
			plt.ylim((-ylim2, ylim2))
			#y = highpass(d_sign[x])
			y = np.abs(np.fft.fft(d_sign[x], axis=1))
			plt.plot(y[i+1])

		x = np.random.randint(rtd[1] * 8 * 12)
		for i in range(6):
			plt.subplot(6, 6, i+13)
			plt.ylim((-ylim, ylim))
			plt.plot(d_idle[x][i+1])
			plt.subplot(6, 6, i+19)
			plt.ylim((-ylim2, ylim2))
			#y = highpass(d_idle[x])
			y = np.abs(np.fft.fft(d_idle[x], axis=1))
			plt.plot(y[i+1])

		x = np.random.randint(rtd[2] * 12)
		for i in range(6):
			plt.subplot(6, 6, i+25)
			plt.ylim((-ylim, ylim))
			plt.plot(d_wand[x][i+1])
			plt.subplot(6, 6, i+31)
			plt.ylim((-ylim2, ylim2))
			#y = highpass(d_wand[x])
			y = np.abs(np.fft.fft(d_wand[x], axis=1))
			plt.plot(y[i+1])
		plt.show()

# classify
if procedure == 'classify':
	d_sign, d_idle, d_wand = pickle.load(open('data/_detect_sample.data', 'rb'))
	print(len(d_sign), len(d_idle), len(d_wand))
	
	def feature_acc(f, x):
		#x = (x - x.mean()) / x.std()
		f.append(x.max())
		f.append(x.min())
		f.append(x.max()/x.min())
		#f.append(x.mean())
		#f.append(x.std())

	def feature_rot(f, x):
		#x = (x - x.mean()) / x.std()
		f.append(x.max())
		f.append(x.min())
		f.append(x.max()/x.min())
		f.append(x.mean())
		#f.append(x.std())

	def feature_freq(f, y):
		yy = y[6:15]
		f.append(yy.max())
		f.append(yy.sum())
		#f.append(yy.mean())
		#f.append(yy.std())

	def feature(x):
		f = []
		#x = highpass(x, 0.1)
		feature_acc(f, x[1])
		feature_acc(f, x[2])
		feature_acc(f, x[3])
		feature_rot(f, x[4])
		feature_rot(f, x[5])
		feature_rot(f, x[6])
		y = np.abs(np.fft.fft(x, axis=1))
		feature_freq(f, y[1])
		feature_freq(f, y[2])
		feature_freq(f, y[3])
		feature_freq(f, y[4])
		feature_freq(f, y[5])
		feature_freq(f, y[6])
		return f

	valitype = 'cross user'

	if valitype == 'merge':
		data = []
		label = []
		for i in range(len(d_sign)):
			data.append(feature(d_sign[i]))
			label.append(0)
		for i in range(len(d_idle)):
			data.append(feature(d_idle[i]))
			label.append(1)
		for i in range(len(d_wand)):
			data.append(feature(d_wand[i]))
			label.append(1)

		print('training...\t', end='')
		t = time.time()
		clf = svm.SVC()
		clf.fit(data, label)
		pickle.dump(clf, open('detector', 'wb'))
		print(time.time() - t)

		print('predicting...\t', end='')
		t = time.time()
		result = clf.predict(data)
		print(time.time() - t)

		c = plot_confusion(label, result, False, False)
		plot_label_acc(label, result)
		#print('acc:', 1.0 * (c[0,0]+c[1,1]+c[1,2]+c[2,1]+c[2,2]) / c.sum())
		print('acc:', 1.0 * (c[0,0]+c[1,1]) / c.sum())

	elif valitype == 'cross user':
		# cross validation
		pr_sum = np.zeros((2,2))
		acc_sum = 0
		m = 12

		for k in range(m):
			#print('\nfold:', k)
			data_t = []
			data_v = []
			label_t = []
			label_v = []

			n = len(d_sign)
			for i in range(n):
				if i >= n/m*k and i < n/m*(k+1):
					data = data_v
					label = label_v
				else:
					data = data_t
					label = label_t
				data.append(feature(d_sign[i]))
				label.append(0)

			n = len(d_idle)
			for i in range(n):
				if i >= n/m*k and i < n/m*(k+1):
					data = data_v
					label = label_v
				else:
					data = data_t
					label = label_t
				data.append(feature(d_idle[i]))
				label.append(1)

			n = len(d_wand)
			for i in range(n):
				if i >= n/m*k and i < n/m*(k+1):
					data = data_v
					label = label_v
				else:
					data = data_t
					label = label_t
				data.append(feature(d_wand[i]))
				label.append(1)

			clf = svm.SVC()
			clf.fit(data_t, label_t)
			result_v = clf.predict(data_v)
			c = plot_confusion(label_v, result_v, False, False)
			plot_label_acc(label_v, result_v, False)
			p0 = c[0,0] / c[:,0].sum()
			r0 = c[0,0] / c[0].sum()
			p1 = c[1,1] / c[:,1].sum()
			r1 = c[1,1] / c[1].sum()
			acc = 1.0 * (c[0,0]+c[1,1]) / c.sum()
			#print('acc: ', acc)

			pr_sum += np.array([[p0,r0],[p1,r1]])
			acc_sum += acc

		print()
		print(pr_sum / m)
		print(acc_sum / m)

	else:
		pass