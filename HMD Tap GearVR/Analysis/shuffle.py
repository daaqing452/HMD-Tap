import numpy as np
name = [['L', 'LU', 'LD', 'LF'], ['R', 'RU', 'RD', 'RF']]
a = [np.arange(4), np.arange(4)]
np.random.shuffle(a[0])
np.random.shuffle(a[1])
j = int(np.random.random() * 2)
print(j)
for i in range(4):
	print(name[j][a[j][i]])
	print(name[1-j][a[1-j][i]])