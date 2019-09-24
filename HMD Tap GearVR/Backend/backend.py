# -*- coding:utf-8 -*-
import matplotlib.pyplot as plt
import numpy as np
import pickle
import socket
import threading
from sklearn import svm

frames = []
n_frame = 0
n_detect = 0

def feature_detect(a):
	f = []
	def feature_axis(x):
		f.append(x.max())
		f.append(x.min())
		f.append(x.mean())
		f.append(x.std())
	feature_axis(a[0])
	feature_axis(a[1])
	feature_axis(a[2])
	return f

def feature_classify(a):
    f = []
    def feature_axis(x):
        x = (x - x.mean()) / x.std()
        f.append(x.max())
        f.append(x.min())
        i = x.argmax()
        j = x.argmin()
        f.append(i<j)
    feature_axis(a[0])
    feature_axis(a[1])
    feature_axis(a[2])
    feature_axis(a[3])
    feature_axis(a[4])
    feature_axis(a[5])
    return f

def classify(a):
	global client
	feature = feature_classify(a)
	data = np.array([feature])
	res = classifier.predict(data)
	client.send("result " + str(res[0]))
	print(res[0])
	return res[0]

def detect(a):
	global n_detect
	feature = feature_detect(a)
	data = np.array([feature])
	res = detector.predict(data)
	if res == False:
		n_detect += 1
		if n_detect == 2:
			classify(a)
			if False:
				for i in range(6):
					plt.subplot(6, 1, i+1)
					plt.plot(a[i])
				plt.show()
	if res == True:
		n_detect = 0

def parse(s):
	global frames
	global n_frame
	skip = 15
	win = 50
	arr = s.split(' ')
	tag = arr[0]
	if tag == 'frame':
		frame = np.array((float(arr[1]), float(arr[2]), float(arr[3]), float(arr[4]), float(arr[5]), float(arr[6])))
		frames.append(frame)
		if len(frames) == win:
			detect(np.array(frames).T)
			frames = frames[skip:]
		#print(n_frame, frame)
		n_frame += 1

class Client:
	rest_s = ''

	def __init__(self):
		self.ip = socket.gethostbyname(socket.gethostname())
		print('[  self ip   ]:', self.ip)
		self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		Android = True
		if Android:
			self.sock.connect(('192.168.1.250', 10441))
		else:
			self.sock.connect(('192.168.1.240', 10441))
		# self.sock.connect(('192.168.1.240', 10441))
		# t.setDaemon(True)
		t = threading.Thread(target=self.recv_thread)
		t.start()

	def send(self, s):
		self.sock.sendall(bytes(s + '\n', encoding='utf-8'))

	def recv(self, s):
		#print(len(s))
		self.rest_s += s
		lines = self.rest_s.split('\n')
		for i in range(len(lines)-1):
			parse(lines[i])
		self.rest_s = lines[-1]

	def recv_thread(self):
		print('[ connected  ]')
		while True:
			b = self.sock.recv(65536)
			if len(b) <= 0:
				print('[disconnected]')
				break
			s = str(b, encoding='utf-8')
			self.recv(s)

if __name__ == '__main__':
	detector = pickle.load(open('detector', 'rb'))
	classifier = pickle.load(open('classifier', 'rb'))
	client = Client()
	'''while True:
		s = input()
		print('[    send    ]: "%s"' % (s))
		client.send(s)'''