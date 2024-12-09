import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = "resources\\DataSet1_Normalized.csv"

# autoencoder model
class AnomalyDetector(Model):
  def __init__(self, latent_dim, dim):
    super(AnomalyDetector, self).__init__()
    self.encoder = tf.keras.Sequential([
      layers.Dense(7, activation="relu"),
      #layers.Dense(4, activation="relu"),
      layers.Dense(latent_dim, activation="relu")])

    self.decoder = tf.keras.Sequential([
      #layers.Dense(4, activation="relu"),
      layers.Dense(7, activation="relu"),
      layers.Dense(dim, activation="sigmoid")])

  def call(self, x):
    encoded = self.encoder(x)
    decoded = self.decoder(encoded)
    return decoded

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

# train autoencoder
autoencoder = AnomalyDetector(2, 13)
autoencoder.compile(optimizer='adam', loss='mae')
autoencoder.fit(data, data,
          epochs=10,
          batch_size=32,
          validation_data=(data, data),
          shuffle=True,
          verbose=0)

# save trained model to file
autoencoder.export("autoencoder")
#autoencoder.save("autoencoder.keras")
test = "done"