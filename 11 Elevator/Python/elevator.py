# Elevator simulation
# elevator.py - Simulates one elevator
#
# 2017-09-03 PV

import math

# Values for Elevator.direction
GOING_ANY = 0
GOING_UP = 1
GOING_DOWN = -1


class Elevator(object):
    """Simulates one elevator"""
    a = 1   # m/s², acceleration
    v = 2   # m/s, speed
    h = 3   # m, floor height

    tacc = v/a              # s, time to accelerate from 0 to v
    dacc = (a*tacc**2)/2    # m, distance to accelerate from 0 to v

    tdoor = 1               # s, Time to open or close doors
    tinout = 2              # s, Time for user to go in or out (per user)


    def __init__(self, id, numFloors):
        self.id = id
        self.numFloors = numFloors
        self.level = 0
        self.direction = GOING_ANY
        self.doorsOpened = False
        self.load = 0       # 0 users in the elevator

    def __str__(self):
        return f"Elevator(id={self.id})"

    def __repr__(self):
        return self.__str__()

    def time_from_distance(self, d):
        if d>2*Elevator.dacc:
            return 2*Elevator.tacc + (d-2*Elevator.tacc)/Elevator.v
        else:
            return 2*math.sqrt(d/Elevator.a)

    def estimated_time_to_service(self, enter, exit):
        if self.direction == GOING_ANY:
            t = 0
            if self.level==enter:
                if not self.doorsOpened: t = tdoor
                return t
            if self.doorsOpened: t += tdoor     # Closing door
            t += self.time_from_distance(h*abs(self.level-enter))     # Moving
            t += tdoor      # Oopening door
            return t;

        if self.direction == GOING_UP:
            if self.level==enter:
                if self.doorsOpened:
                    return 0
                else:
                    return None
            if enter<self.level:
                return None

            # calculation that depends on current position and expected stops
            # ToDo

        if self.direction == GOING_DOWN:
            if self.level==enter:
                if self.doorsOpened:
                    return 0
                else:
                    return None
            if enter>self.level:
                return None

            # calculation that depends on current position and expected stops
            # ToDo

        return None

