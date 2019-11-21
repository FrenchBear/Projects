# Elevator simulation
# user.py - Represents one elevator user
#
# 2017-09-02 PV

class User(object):
    """Represents one elevator user"""

    def __init__(self, id, arrival, enter, exit):
        self.id = id
        self.arrival = arrival
        self.enter = enter
        self.exit = exit

    def __str__(self):
        return f"User(id={self.id}, arrival={self.arrival}, enter={self.enter}, exit={self.exit})"

    def __repr__(self):
        return self.__str__()

