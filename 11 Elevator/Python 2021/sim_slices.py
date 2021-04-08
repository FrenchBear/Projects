# Simulator 2021
# v2, with regular time slices
# Simulation with actors
# 2021-04-08    PV

# until Python 3.10: https://stackoverflow.com/questions/33533148/how-do-i-type-hint-a-method-with-the-type-of-the-enclosing-class
from __future__ import annotations
from datetime import datetime, timedelta
from typing import Optional, Iterable


class Actor:
    def __init__(self, name: str) -> None:
        self.name = name
        self.act_clock: datetime = datetime(2021, 1, 1)
        self.next_transition_clock: Optional[datetime] = datetime(2021, 1, 1)
        self.state = 'Init'

    # Update internal state according to clock
    def set_clock(self, dt: datetime):
        self.act_clock = dt
        return None

    # React to all other actors
    def react(self, actors: list[Actor]):
        pass


class Car(Actor):
    count = 0
    car_accel = 1   # 1_m.s⁻²
    car_decel = 1

    def __init__(self) -> None:
        Car.count += 1
        super().__init__(f'Car #{Car.count}')
        self.start_dist: float = 0
        self.start_speed: float = 0
        self.dist: float = 0
        self.speed: float = 0
        self.accel: float = 0
        self.start_transition_clock: datetime = datetime(2021,1,1)

    def set_clock(self, dt: datetime):
        self.act_clock = dt

        if not self.next_transition_clock:
            return

        if dt <= self.next_transition_clock:
            t = (dt-self.start_transition_clock).total_seconds()
            if self.state == 'Acceleration':
                self.speed = self.start_speed + t*self.accel
                self.dist = self.start_dist + 0.5*self.accel*t**2
            elif self.state == 'Cruise':
                # speed is constant
                self.dist = self.start_dist + t*self.speed
            elif self.state == 'Deceleration':
                self.speed = self.start_speed + t*self.accel
                self.dist = self.start_dist + self.start_speed*t + 0.5*self.accel*t**2
                if self.speed < 0:
                    self.speed = 0
            if dt < self.next_transition_clock:
                print(f'{dt}: in state {self.state}, dist={self.dist}, speed={self.speed}, accel={self.accel}, next transition={self.next_transition_clock}')
                return

        self.start_transition_clock = dt
        if self.state == 'Init':
            self.next_transition_clock = dt+timedelta(seconds=2)
            self.state = 'WarmUp'
        elif self.state == 'WarmUp':
            self.next_transition_clock = dt+timedelta(seconds=10)
            self.state = 'Acceleration'
            self.accel = Car.car_accel
        elif self.state == 'Acceleration':
            self.next_transition_clock = dt+timedelta(seconds=30)
            self.state = 'Cruise'
            self.start_dist = self.dist
            self.start_speed = self.speed
            self.accel = 0
        elif self.state == 'Cruise':
            self.next_transition_clock = dt+timedelta(seconds=10)
            self.state = 'Deceleration'
            self.start_dist = self.dist
            self.accel = -Car.car_decel
        elif self.state == 'Deceleration':
            self.next_transition_clock = None
            self.accel = 0
            self.speed = 0
            self.state = 'End'
        print(f'{dt}: transition to state {self.state}, dist={self.dist}, speed={self.speed}, accel={self.accel}, next transition={self.next_transition_clock}')

    def react(self, actors: list[Actor]):
        pass


class SimMan:
    def __init__(self) -> None:
        # By default, simulation starts on 1st Jan 2021
        self.set_clock(datetime(2021, 1, 1))
        self.actors: list[Actor] = []
        self.cycle = 0

    def set_clock(self, dt: datetime):
        self.sim_clock = dt

    def add_actor(self, a: Actor):
        self.actors.append(a)

    def main_loop(self):
        while True:
            self.cycle += 1
            #print('\nMain loop, cycle', self.cycle, ' clock:', self.sim_clock)
            for a in self.actors:
                a.set_clock(self.sim_clock)
            if all(a.state == 'End' for a in self.actors):
                break
            for a in self.actors:
                a.react(self.actors)
            sim.set_clock(sim.sim_clock+timedelta(seconds=1))

        print('Simulation ends at cycle', self.cycle)


if __name__ == '__main__':
    sim = SimMan()
    c1 = Car()
    sim.add_actor(c1)
    sim.main_loop()
