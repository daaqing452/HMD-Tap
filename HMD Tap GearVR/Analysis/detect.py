import matplotlib.pyplot as plt
import numpy as np
from scipy import signal
from sklearn import svm
from utils import *
import pickle

al, bl, ar, br, af, bf, au, blu, bru, at, bt, ab, bbl, bbr = read_0(win_t=None)
blu.extend(bru)
bbl.extend(bbr)

data = []
label = []

def feature(a):
	f = []
	def feature_axis(x):
		f.append(x.max())
		f.append(x.min())
		f.append(x.mean())
		f.append(x.std())
	feature_axis(a[1])
	feature_axis(a[2])
	feature_axis(a[3])
	#feature_axis(a[4])
	#feature_axis(a[5])
	#feature_axis(a[6])
	return f

for a,b in [(al,bl),(ar,br),(af,bf),(au,blu),(at,bt),(ab,bbl)]:
	n = a.shape[1]
	c = np.zeros((n))
	for l,r in b:
		c[l:r] = np.ones((r-l))
	m = len(b)
	for i in range(m*20):
		x = np.random.randint(0, n-30)
		tag = (c[x:x+30].sum() == 0)
		data.append(feature(a[:,x:x+30]))
		label.append(tag)

data = np.array(data)
label = np.array(label)
print('n:', data.shape[0])

#cross_validation(data, label)

clf = svm.SVC()
clf.fit(data, label)
pickle.dump(clf, open('detector', 'wb'))

result = clf.predict(data)
tp = np.logical_and(1-label, 1-result).sum()
fp = np.logical_and(label, 1-result).sum()
fn = np.logical_and(1-label, result).sum()
tn = np.logical_and(label, result).sum()
print(' accuracy: %.5lf (%d/%d)' % (100.0*(tp+tn)/(tp+tn+fp+fn), tp+fn, tp+tn+fp+fn))
print('precision: %.5lf (%d/%d)' % (100.0*tp/(tp+fp), tp, tp+fp))
print('   recall: %.5lf (%d/%d)' % (100.0*tp/(tp+fn), tp, tp+fn))
print('       f1: %.5lf' % (200.0*tp/(tp+tp+fp+fn)))