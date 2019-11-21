# Elevator simulation
# pool.py - Represents all elevators in the building
#
# 2017-09-03 PV

from elevator import *

class Pool(object):
    """Represents all elevators in the building"""

    def __init__(self, numElevators, numFloors):
        self.numElevators = numElevators
        self.elevators = [Elevator(i, numFloors) for i in range(numElevators)]

    def __str__(self):
        return f"Pool({self.elevators})"

    def __repr__(self):
        return self.__str__()

    def handle_request(self, enter, exit):
        for elevator in self.elevators:
            pass
            #if elevator.can_service(enter, exit):
            #    print(f"{elevator} can handle this request")
            #    return

        ## Try to put an elevator in motion to serve this request.
        #for elevator in self.elevators:
        #    if elevator.can_service(enter, exit):
        #        print(f"{elevator} will handle this request")
        #        return

        # If we can't, no problem, user will have to wait.
        print("No elevator is available right now to handle this request")

