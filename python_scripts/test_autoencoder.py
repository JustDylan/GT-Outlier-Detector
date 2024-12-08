import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = "resources\\DataSet1_Normalized.csv"

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

# load autoencoder from model trained by train_autoencoder
autoencoder = tf.saved_model.load("autoencoder")
#autoencoder = tf.keras.models.load_model("autoencoder.keras")

decoded_data = autoencoder.serve(data)

decoded_error = [vDiff(decoded_data[i], data[i]) for i in range(0, len(decoded_data))]

test = "done"