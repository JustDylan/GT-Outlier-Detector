import matplotlib.pyplot as plt
import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = "DataSet1_Normalized.csv"

print(tf.version.VERSION)

# average squared difference between float arrays
def vDiff(arr1, arr2):
	length = min(len(arr1), len(arr2))
	return sum([((arr1[i] - arr2[i])**2)/length for i in range(0, length)], 0)


# read data from csv only including columns X1 through X13
data = {}
with open(DATA_PATH) as csvfile:
	csvreader = csv.reader(csvfile, delimiter=",")
	raw_data = list(csvreader)
	
	# remove header row
	raw_data = raw_data[1:]
	
	# remove first 2 columns and parse floats
	for i in range(0, len(raw_data)):
		raw_data[i] = raw_data[i][2:]
		raw_data[i] = [float(elem) for elem in raw_data[i]]
		
	data = np.array(raw_data, dtype="float32")

#print(type(data))
#print(np.shape(data))

# load autoencoder from model trained by train_autoencoder
autoencoder = tf.saved_model.load("autoencoder")
#autoencoder = tf.keras.models.load_model("autoencoder.keras")

# arificially insert outlier
data[400][0] += 0.4
decoded_data = autoencoder.serve(data)

# graph of one decoded row

plt.grid()
plt.plot(np.arange(13), data[400])
plt.plot(np.arange(13), decoded_data[400])
plt.show()

plt.grid()
plt.plot(np.arange(13), data[500])
plt.plot(np.arange(13), decoded_data[500])
plt.show()


decoded_error = [vDiff(decoded_data[i], data[i]) for i in range(0, len(decoded_data))]

# X1 vs decoded X1

# original data for X1
sensor1 = [row[0] for row in data]

# decoded X1
sensor1_decoded = [row[0] for row in decoded_data]

plt.grid()
plt.title("Sensor X1")

# original X1 data
plt.scatter(np.arange(len(sensor1)), sensor1)

# reconstructed X1 data
plt.scatter(np.arange(len(sensor1)), sensor1_decoded)
plt.show()


# predicted error vs row number
plt.grid()
plt.title("Averaged squared diff vs Row #")

plt.scatter(np.arange(len(decoded_error)), decoded_error)

plt.show()
