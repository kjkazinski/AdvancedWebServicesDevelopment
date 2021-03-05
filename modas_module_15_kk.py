from gpiozero import MotionSensor, LED, Button
from picamera import PiCamera
from time import sleep
import requests
import json
import jwt
import datetime
from random import randint
import datetime as dt

class Modas:
    def __init__(self):
        # init PiCamera
        self.camera = PiCamera()
        # set camera resolution
        #self.camera.rotation = 180
        self.camera.resolution = (1024,768)
        # init green, red LEDs
        self.green = LED(24)
        self.red = LED(23)
        # init button
        self.button = Button(8)
        # init PIR
        self.pir = MotionSensor(25)
        
        # when button  is released, toggle system arm / disarm
        self.button.when_released = self.toggle
        
        # system is disarmed by default
        self.armed = False
        self.disarm_system()
        
    def init_alert(self):
        self.green.off()
        self.red.blink(on_time=.25, off_time=.25, n=None, background=True)
        print("motion detected")
        
        # determine current date/time
        t = dt.datetime.now()
        
        # determine the number of seconds that have elapsed since midnight
        s = t.second + (t.minute * 60) + (t.hour * 60 * 60)

        # Take photo
        self.snap_photo(t)
        # Add the event to the API and log
        self.update_modas(t)

        # delay
        sleep(2)
        
    def update_modas(self, objTime):
        LocationId = randint(1, 3)
        StatusCode = self.update_modas_api(objTime, LocationId)
        self.write_log(objTime, LocationId, StatusCode)

    def update_modas_api(self, objTime, LocationId):

        token = self.Token()

        if token != None:
            # create a new event - replace with your API
            url = 'https://modasapi.azurewebsites.net/api/event/'
            headers = { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token }
            t_json = "{0}-{1}-{2}T{3}:{4}:{5}".format(objTime.strftime("%Y"), objTime.strftime("%m"), objTime.strftime("%d"), objTime.strftime("%H"), objTime.strftime("%M"), objTime.strftime("%S"))
            Flagged = False
            
            payload = { 'timestamp': t_json, 'flagged': Flagged, 'locationId': LocationId }
            #print(payload)
            
            # post the event
            r = requests.post(url, headers=headers, data=json.dumps(payload))
            
            return r.status_code
            #print(r.status_code)
            #print(r.json())
        else:
            return 0
        
    def Token(self):
        try:
            # attempt to open the file token.json for reading
            f = open("token.json", "r")
            # assuming the file can be opened, read the file into a variable
            # convert the string into json
            encoded_token = json.loads(f.read())
            f.close()
            # using jwt package decode the token
            # all we are looking for is the token expiration (exp)
            unencoded_token = jwt.decode(encoded_token["token"], verify = False)

            # save the exp date (remember it is in Unix timestamp format)
            exp_epoch = int(unencoded_token["exp"])

            self.displayToken(exp_epoch)

            # get current date/time
            today = datetime.datetime.now()
            # convert current date/time to UNIX timestamp (Epoch)
            current_epoch = int(today.timestamp())

            # subtract the current date/time from the exp date/time
            # if the difference is positive the token is not yet expired
            diff = exp_epoch - current_epoch

            if diff > 0:
                return encoded_token["token"]
            else:
                #print("Token is expired")
                return None
        except:
            print("An error occurred")
            return None
 
    def displayToken(self, exp_epoch):
        
        # get current date/time
        today = datetime.datetime.now()
        # convert current date/time to UNIX timestamp (Epoch)
        current_epoch = int(today.timestamp())

        # subtract the current date/time from the exp date/time
        # if the difference is positive the token is not yet expired
        diff = exp_epoch - current_epoch
        if diff > 0:
            # days = seconds / (sec/min) / (min/hr) / (hr/day)
            days = diff / 60 / 60 / 24
            #print("Token is valid for: {0} days".format(int(days)))
            # TODO: have students write this the following
            # hours = ???
            hours = (days - int(days)) * 24
            # minutes = ???
            minutes = (hours - int(hours)) * 60
            # seconds = ???
            seconds = (minutes - int(minutes)) * 60
            print("Token is valid for: {0} days, {1} hours, {2} minutes, {3} seconds.".format(int(days), int(hours), int(minutes), int(seconds)))
            # display exp date/time
            print("\tToken expires (epoch): {0}".format(exp_epoch))
            # display current date/time
            print("\tToday (epoch):         {0}".format(current_epoch))
        else:
            print("Token is expired")


    def write_log(self, objTime, LocationId, StatusCode):
        
        # File Name 2021-12-01.log        
        filename ="{0}-{1}-{2}.log".format(objTime.strftime("%Y"), objTime.strftime("%m"), objTime.strftime("%d"))

        f = open(filename, "a")
        
        # Format the timestamp
        t_json = "{0}-{1}-{2}T{3}:{4}:{5}".format(objTime.strftime("%Y"), objTime.strftime("%m"), objTime.strftime("%d"), objTime.strftime("%H"), objTime.strftime("%M"), objTime.strftime("%S"))
        
        #print("{0},{1},{2},{3}".format(t_json,"False",LocationId,StatusCode))
        f.write("{0},{1},{2},{3}\n".format(t_json,"False",LocationId,StatusCode))
        f.close()

    def snap_photo(self, objTime):
        
        # determine the number of seconds that have elapsed since midnight
        s = objTime.second + (objTime.minute * 60) + (objTime.hour * 60 * 60)

        # use the date/time to generate a unique file name for photos (1/1/2018~21223.png)
        self.camera.capture("/home/pi/Desktop/{0}~{1}.jpg".format(objTime.strftime("%Y-%m-%d"), s))

    def reset(self):
        self.red.off()
        self.green.on()
        
    def toggle(self):
        self.armed = not self.armed
        if self.armed:
            self.arm_system()
        else:
            self.disarm_system()
            
    def arm_system(self):
        print("System armed in 3 seconds")
        self.red.off()
        # enable camera
        self.camera.start_preview()
        # 3 second delay
        self.green.blink(on_time=.25, off_time=.25, n=6, background=False)
        # enable PIR
        self.pir.when_motion = self.init_alert
        self.pir.when_no_motion = self.reset
        self.green.on()
        print("System armed")
        
    def disarm_system(self):
        # disable PIR
        self.pir.when_motion = None
        self.pir.when_no_motion = None
        # disable camera
        self.camera.stop_preview()
        self.red.on()
        self.green.off()
        print("System disarmed")

m = Modas()

try:
    # program loop
    while True:
        sleep(.001)
# detect Ctlr+C
except KeyboardInterrupt:
    if m.armed:
        m.disarm_system()