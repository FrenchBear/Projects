# Elevator simulation
# event.py - Class hierarchy to medelize events
#
# 2017-09-03 PV

class Event(object):
    """Base class for ann simulation events"""
    def __init__(self, time, type):
        self.type = type
        self.time = time

    def __str__(self):
        return "Event(time=%s, type=%d)" % (self.time, self.type)

    def __repr__(self):
        return str(self)


USER_ARRIVAL_EVENT = 1

class UserArrivalEvent(Event):
    def __init__(self, time, user):
        super().__init__(time, USER_ARRIVAL_EVENT)
        self.user = user

    def __str__(self):
        return f"UserArrivalEvent(time={self.time}, user={self.user})"

    def __repr__(self):
        return str(self)
