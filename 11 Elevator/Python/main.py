# Elevator simulation
# main.py - Starting module
#
# 2017-09-02    PV

from building import *
from scheduler import *

b = Building(numFloors=10, numElevators=1, numUsers=2)
sc = Scheduler(b)
sc.run_simulation()

print()
print("Simulation ended at time ", sc.time)


