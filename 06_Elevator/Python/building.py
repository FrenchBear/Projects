# Elevator simulation
# building.py
#
# 2017-09-02 PV

from random import *
from pool import *
from elevator import *
from floor import *
from user import *


class Building(object):
    """Main simulation object, represents a building with elevators, floors and users"""

    def generate_pool(self, numElevators, numFloors):
        self.pool = Pool(numElevators, numFloors)
        print(self.pool)


    def generate_floors(self, numFloors):
        self.floors = [Floor(i) for i in range(numFloors)]
        #print(self.floors)


    def generate_users(self, numUsers, numFloors):
        self.users = []
        for i in range(numUsers):
            # 40% of time, enter=floor 0, exit=floor 1..numFloors-1
            # 40% of time, enter=floor 1..numFloors-1, exit=floor 0
            # 20% of time, enter and exit = 1..numFloors-1 (enter!=exit)
            r = self.rnd.next_double()
            if r < 0.4:
                enter = 0
                exit = 1 + self.rnd.next_int(numFloors - 1)
            elif r < 0.8:
                enter = 1 + self.rnd.next_int(numFloors - 1)
                exit = 0
            else:
                while True:
                    enter = 1 + self.rnd.next_int(numFloors - 1)
                    exit = 1 + self.rnd.next_int(numFloors - 1)
                    if enter != exit: break

            arrival = self.rnd.next_int(30 * (i+1))
            user = User(i, arrival, enter, exit)
            self.users.append(user)
        #print(self.users)


    def __init__(self, numFloors, numElevators, numUsers):
        self.rnd = UniformIntRandom(0)

        self.numFloors = numFloors
        self.numElevators = numElevators
        self.numUsers = numUsers

        self.generate_pool(numElevators, numFloors)
        self.generate_floors(numFloors)
        self.generate_users(numUsers, numFloors)

