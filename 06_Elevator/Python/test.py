# Elevator simulation
# test.py - Various simulation tests
#
# 2017-09-04 PV
from elevator import *

def test_elevator_time_from_distance():
    el = Elevator(0, 10)
    for i in range(19):
        print(i / 2.0, el.time_from_distance(i / 2.0))
    assert(el.time_from_distance(0) == 0)
    assert(el.time_from_distance(0.5) == math.sqrt(2))
    assert(el.time_from_distance(4) == 4)
    assert(el.time_from_distance(8) == 6)