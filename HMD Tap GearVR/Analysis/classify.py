# -*- coding:utf-8 -*-
import matplotlib.pyplot as plt
import numpy as np
import pickle
from sklearn import svm, ensemble, neighbors, linear_model
from utils import *

if len(sys.argv) >= 2:
	mode = sys.argv[1]
else:
	mode = 'merge'

iteration = 1

def read_all(name):
	ddir = './data/' + name + '/'
	a = []
	a.append(read(ddir + 'left.txt'))			# 0
	a.append(read(ddir + 'right.txt'))			# 1
	a.append(read(ddir + 'upper-left.txt'))		# 2
	a.append(read(ddir + 'upper-right.txt'))	# 3
	a.append(read(ddir + 'lower-left.txt'))		# 4
	a.append(read(ddir + 'lower-right.txt'))	# 5
	a.append(read(ddir + 'front-left.txt'))		# 6
	a.append(read(ddir + 'front-right.txt'))	# 7
	b = [naive_detect(ai) for ai in a]

	s = ''
	for bi in b:
		s += str(len(bi)) + ' '
	print(s)
	return a, b

p = []
#names = ['yangzhican']
#names = ['guyizheng','huyuan', 'liguohao', 'luyiqin', 'mengqi', 'shijiaxin', 'sunke', 'wangruolin', 'weixiaoying', 'xiawu', 'xiexiaohui', 'yanyukang']
names = ['guyizheng', 'huyuan', 'luyiqin', 'mengqi', 'shijiaxin', 'sunke', 'weixiaoying', 'xiawu', 'xiexiaohui', 'yangzhican', 'yanyukang' , 'zhoulinjun']
print('Reading...')
for name in names:
	#a, b = read_all(name)
	a, b = pickle.load(open('data/' + name + '.data', 'rb'))
	a = [a[1], a[2], a[4]]
	b = [b[1], b[2], b[4]]
	p.append((a, b))
print('-' * 80)

'''def dig(b, i):
	bb = b[:i]
	bb.extend(b[i+1:])
	return bb

b[2] = dig(b[2], 25)
b[2] = dig(b[2], 24)
b[2] = dig(b[2], 6)
b[2] = dig(b[2], 5)
b[2] = dig(b[2], 4)

b[3] = dig(b[3], 3)

b[6] = b[6][1:]

plot_segmentation(a[5], b[5])
pickle.dump((a, b), open('data/' + names[0] + '.data', 'wb'))
xxx'''

def feature(a):
	f = []
	def feature_axis(x, norm=True):
		#if norm: x = (x - x.mean()) / x.std()
		f.append(x.max())
		f.append(x.min())
		f.append(x.max()/x.min())
		i = x.argmax()
		j = x.argmin()
		f.append(i<j)
		#f.append(i-j)

		'''f.append(x[:7].sum())
		f.append(x[7:15].sum())
		f.append(x[15:23].sum())
		f.append(x[23:].sum())'''
	
		f.append(x[:10].max())
		f.append(x[:10].min())
		f.append(x[:10].sum())
		f.append(x[10:20].max())
		f.append(x[10:20].min())
		f.append(x[10:20].sum())
		f.append(x[20:].max())
		f.append(x[20:].min())
		f.append(x[20:].sum())

	feature_axis(a[1])
	feature_axis(a[2])
	feature_axis(a[3])
	feature_axis(a[4])
	feature_axis(a[5])
	feature_axis(a[6])
	return f

# feature selection
def make_data(a, b, tag, store=None):
	global data, label
	if store is None:
		data2 = data
		label2 = label
	else:
		data2, label2 = store
	i = 0
	for (l,r) in b:
		f = feature(a[:, l:r])
		data2.append(f)
		label2.append(tag)
		i += 1

clf = svm.SVC()
#clf = ensemble.RandomForestClassifier(n_estimators=100)
#clf = neighbors.KNeighborsClassifier(n_neighbors=10)
#clf = linear_model.SGDClassifier()

if mode == 'individual':
	for j in range(len(names)):
		a, b = p[j]
		data = []
		label = []
		for i in range(len(a)):
			make_data(a[i], b[i], i)
		data = np.array(data)
		label = np.array(label)
		print(names[j] + ':')
		for it in range(iteration):
			label2, result = cross_validation(data, label, clf)
			plot_label_acc(label2, result)
		#clf.fit(data, label)
		#pickle.dump(clf, open('classifier', 'wb'))
		#result = clf.predict(data)
		#plot_label_acc(data, label)
		print('-' * 80)
elif mode == 'merge':
	data = []
	label = []
	for j in range(len(names)):
		a, b = p[j]
		for i in range(len(a)):
			make_data(a[i], b[i], i)
	data = np.array(data)
	label = np.array(label)
	cross_validation(data, label, clf)
	clf.fit(data, label)
	pickle.dump(clf, open('classifier', 'wb'))
	result = clf.predict(data)
	c = plot_confusion(label, result)
	print(c/100)
	plot_label_acc(label, result)
elif mode == 'cross user':
	u = len(names)
	loc = 2
	acc_mean = 0
	pss = np.zeros((loc))
	rss = np.zeros((loc))
	for i in range(u):
		data = []
		label = []
		for j in range(u):
			if i != j:
				a, b = p[j]
				for k in range(loc):
					make_data(a[k], b[k], k)
		a2, b2 = p[i]
		data2 = []
		label2 = []
		for k in range(loc):
			make_data(a2[k], b2[k], k, store=(data2, label2))
		data2 = np.array(data2)
		label2 = np.array(label2)
		clf.fit(data, label)
		result2 = clf.predict(data2)
		acc = get_acc(label2, result2)
		acc_mean += acc
		ps, rs = plot_label_acc(label2, result2, show=False)
		pss += np.array(ps)
		rss += np.array(rs)
		print('user', i, acc)
	print('overall-acc:', acc_mean / u)
	print(pss / u)
	print(rss / u)
