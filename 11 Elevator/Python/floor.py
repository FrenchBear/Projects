# Elevator simulation
# floor.py - Represents one level in the building
#
# 2017-09-02 PV

class Floor(object):
    """Represents one level in the building"""

    def __init__(self, id):
        self.id = id

    def __str__(self):
        return f"Floor(id={self.id})"

    def __repr__(self):
        return self.__str__()
