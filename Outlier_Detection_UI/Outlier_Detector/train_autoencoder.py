import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = "DataSet1_Normalized.csv"

print(tf.version.VERSION)