from picamera import PiCamera
from gpiozero import Button
from datetime import datetime
import time


button = Button(17)
camera = PiCamera()

camera.start_preview()
time.sleep(2)
today = datetime.now()
filename = '/home/pi/image_' + str(today) + '.jpg'
camera.capture(filename)
camera.stop_preview()
