# Elevator simulation
# scheduler.py - Event scheduler, core of simulation
#
# 2017-09-03 PV
from building import *
from event import *
from user import *


class Scheduler(object):
    """Elevator sinulation events scheduler"""

    # Set simulation master clock time
    def set_time(self, time):
        assert time >= self.time
        if time > self.time:
            self.time = time
            print("It's now time ", time)


    def build_events_queue(self):
        self.eventsQueue = [UserArrivalEvent(time=user.arrival, user=user)
                            for user in self.b.users]
        #print(self.eventsQueue)


    def __init__(self, b):
        self.b = b
        self.time = 0
        self.build_events_queue()


    def __str__(self):
        return "Scheduler()"

    def __repr__(self):
        return self.__str__()


    def process_user_arrival_event(self, e):
        print(f"process_user_arrival_event({e})")
        enter = e.user.enter
        exit = e.user.exit

        # Ask the pool to handle this request.  If no elevator can handle it,
        # user will have to wait.
        self.b.pool.handle_request(enter, exit)


    def process_event(self, e):
        if e.type == USER_ARRIVAL_EVENT: self.process_user_arrival_event(e)
        else:
            printf("Unknown event type ", e.type)
            assert(false)


    def run_simulation(self):
        while len(self.eventsQueue) > 0:
            nextEvent = min(self.eventsQueue, key=lambda e: e.time)
            self.eventsQueue.remove(nextEvent)
            self.set_time(nextEvent.time)
            self.process_event(nextEvent)

