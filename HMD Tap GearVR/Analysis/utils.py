import matplotlib.pyplot as plt
import numpy as np
import sys
from scipy import signal
from sklearn import svm

def read(filename=None, is_acc=True):
    if filename is None:
        filename = sys.argv[1]
    f = open(filename, 'r')
    x = []
    y = []
    z = []
    rx = []
    ry = []
    rz = []
    while True:
        line = f.readline()
        if len(line) == 0: break
        arr = line.split(' ')
        timestamp = arr[0]
        label = arr[1]
        if label == 'frame':
            x.append(float(arr[2]))
            y.append(float(arr[3]))
            z.append(float(arr[4]))
            rx.append(float(arr[5]))
            ry.append(float(arr[6]))
            rz.append(float(arr[7]))
    f.close()
    a = np.array([x, x, y, z, rx, ry, rz])
    if not is_acc:
        a = np.diff(a, 2, axis=1)
    n = a.shape[1]
    a[0] = np.arange(n)
    return a

def naive_detect(a, fs=100.0, win_t=0.3, win_bias='m', thres=None):
    if thres is None: thres = 0.05
    thres = [0, thres, thres, thres]
    idle_span = 0.5 * fs
    n = a.shape[1]
    b = []
    mStart = mEnd = -1
    idle = True
    for i in range(n):
        mu = (abs(a[1,i]) > thres[1] or abs(a[2,i]) > thres[2] or abs(a[3,i]) > thres[3])
        if ((idle and mu) or i == n-1) and mStart > -1:
            b.append((mStart, mEnd + 1))
        if mu:
            mEnd = i
            if idle:
                mStart = i
                idle = False
        else:
            if i - mEnd > idle_span:
                idle = True
    if win_t is not None:
        win = int(win_t * fs)
        bb = []
        for i in range(len(b)):
            l, r = b[i]
            k = r - l
            if k < win:
                if win_bias == 'l':
                    r += win - k
                elif win_bias == 'm':
                    l -= (win - k) >> 1
                    r += win - (r - l)
                elif win_bias == 'r':
                    l -= win - k
            if k > win:
                r -= k - win
            if l >= 0 and r < n:
                bb.append((l, r))
    return bb

def get_acc(label, result):
    n = label.shape[0]
    wa_index = np.nonzero(label != result)[0]
    wa = len(wa_index)
    acc = 100 * (1 - wa/(1.0*n))
    return acc

def cross_validation(data, label, clf=svm.SVC(), fold=10):
    n = data.shape[0]
    arr = np.arange(n)
    np.random.shuffle(arr)
    data = np.array([data[i] for i in arr])
    label = np.array([label[i] for i in arr])
    acc_mean = 0
    result = np.zeros((n))
    for i in range(fold):
        l = int(n * i / fold)
        r = int(n * (i+1) / fold)
        data2 = np.concatenate((data[:l], data[r:]))
        label2 = np.concatenate((label[:l], label[r:]))
        clf.fit(data2, label2)
        result[l:r] = clf.predict(data[l:r])
        acc_mean += get_acc(label[l:r], result[l:r])
        #print('fold %d: %lf' % (i, acc))
    print('cross-validation acc:', acc_mean / fold)
    return label, result

def time2freq(a, fs, segment=None):
    n = a.shape[1]
    af = np.fft.fft(a, axis=1)
    af[0] = np.arange(n) * fs / n
    if segment is not None:
        af = af[:, int(segment[0] / fs * n):int(segment[1] / fs * n)]
    return af

def highpass(a, btc=0.4):
    coeff_b, coeff_a = signal.butter(3, btc, 'highpass')
    return signal.filtfilt(coeff_b, coeff_a, a, axis=1)



def plot_time(a, idx=None):
    if idx is None:
        idx = np.array(range(a.shape[0]-1)) + 1
    n = idx.shape[0]
    for i in range(n):
        plt.subplot(n, 1, i+1)
        plt.plot(a[0], a[idx[i]])
    plt.show()

def plot_time_freq(a, fs, idx=None):
    if idx is None:
        idx = np.array(range(3)) + 1
    af = time2freq(a, fs)
    n = np.array(idx).shape[0]
    for i in range(n):
        plt.subplot(n*2, 1, i+1)
        plt.plot(a[0], a[idx[i]])
    for i in range(n):
        plt.subplot(n*2, 1, i+n+1)
        plt.plot(af[0], np.abs(af[idx[i]]))
    plt.show()

def plot_detail(a):
    row = 9
    pl = min(row, len(a))
    for i in range(pl):
        for j in range(6):
            plt.subplot(row, 6, i*6+j+1)
            plt.plot(a[i][j+1])
    plt.show()

def plot_confusion(a, b, confusion_show=True, plot_show=True):
    a = np.array(a, dtype=int)
    b = np.array(b, dtype=int)
    n = 0
    for i in range(a.shape[0]):
        n = max(n, a[i]+1)
        n = max(n, b[i]+1)
    c = np.zeros((n,n))
    for i in range(a.shape[0]):
    	c[a[i], b[i]] += 1
    if confusion_show:
        print(c)
    if plot_show:
        plt.clf()
        fig = plt.figure()
        ax = fig.add_subplot(111)
        res = ax.imshow(c, interpolation='nearest')
        cb = fig.colorbar(res)
        plt.show()
    return c

def plot_label_acc(label, result, show=True):
    c = plot_confusion(label, result, confusion_show=False, plot_show=False)
    if show:
        print('label\tprecise\trecall')
    ps = []
    rs = []
    for i in range(c.shape[0]):
        precision = c[i, i] / c[:,i].sum()
        recall = c[i, i] / c[i].sum()
        ps.append(precision)
        rs.append(recall)
        if show:
            print('%d\t%.3lf\t%.3lf' % (i, precision*100, recall*100))
    return ps, rs

def plot_segmentation(a, b):
    c = np.zeros((a.shape[1]))
    i = 0
    for (l,r) in b:
        c[l:r+1] = np.ones((r-l+1))*(i//10+10)
        i += 1
    plt.subplot(411)
    plt.plot(a[0], a[1])
    plt.subplot(412)
    plt.plot(a[0], a[2])
    plt.subplot(413)
    plt.plot(a[0], a[3])
    plt.subplot(414)
    plt.plot(a[0], c)
    plt.show()


def read_0(win_t=0.3):
    print('-' * 80)
    al = read(filename='./data/100/left.txt')
    bl = naive_detect(al, fs=100.0, win_t=win_t)
    print('Read left:', len(bl))

    ar = read(filename='./data/100/right.txt')
    br = naive_detect(ar, fs=100.0, win_t=win_t)
    print('Read right:', len(br))

    af = read(filename='./data/100/front.txt')
    bf = naive_detect(af, fs=100.0, win_t=win_t)
    print('Read front:', len(bf))

    au = read(filename='./data/100/leftup@rightup.txt')
    bu = naive_detect(au, fs=100.0, win_t=win_t)
    blu = bu[:50]
    bru = bu[50:]
    print('Read left-up:', len(blu))
    print('Read right-up:', len(bru))

    at = read(filename='./data/100/top.txt')
    bt = naive_detect(at, fs=100.0, win_t=win_t)
    print('Read top:', len(bt))

    ab = read(filename='./data/100/bottomleft@bottomright.txt')
    bb = naive_detect(ab, fs=100.0, win_t=win_t)
    bbl = bb[:50]
    bbr = bb[50:]
    print('Read bottom-left:', len(bbl))
    print('Read bottom-right:', len(bbr))
    print('-' * 80)

    return al, bl, ar, br, af, bf, au, blu, bru, at, bt, ab, bbl, bbr
