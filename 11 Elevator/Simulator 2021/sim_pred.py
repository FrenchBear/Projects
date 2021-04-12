# Simulator 2021
# v1, with predictive next event time
# Simulation with actors
# 2021-04-08    PV

from datetime import datetime, timedelta
from typing import Optional, Iterable


class Event:
    pass


class Actor:
    def __init__(self, name: str) -> None:
        self.name = name
        self.act_clock: datetime = datetime(2021,1,1)
        self.next_transition_clock: Optional[datetime] = datetime(2021,1,1)
        self.state = 'Init'

    def set_clock(self, dt: datetime) -> Optional[Iterable[Event]]:
        self.act_clock = dt
        return None

    def process_events(self, events: list[Event]):
        pass


class Car(Actor):
    count = 0
    accel = 1   # 1_m.s⁻²

    def __init__(self) -> None:
        Car.count += 1
        super().__init__(f'Car #{Car.count}')

    def set_clock(self, dt: datetime) -> Optional[Iterable[Event]]:
        if self.next_transition_clock and dt>self.next_transition_clock:
            raise ValueError('set_clock dt>next_transition_clock')
        if self.state=='Init':
            self.next_transition_clock += timedelta(seconds=2)
            self.state='WarmUp'
        elif self.state=='WarmUp':
            self.next_transition_clock += timedelta(seconds=10)
            self.state='Acceleration'
        elif self.state=='Acceleration':
            self.next_transition_clock += timedelta(seconds=30)
            self.state='Cruise'
        elif self.state=='Cruise':
            self.next_transition_clock += timedelta(seconds=10)
            self.state='Deceleration'
        elif self.state=='Deceleration':
            self.next_transition_clock = None
            self.state='Stop'
        print(f'{self.name}: state={self.state}, next transition clock={self.next_transition_clock}')

class SimMan:
    def __init__(self) -> None:
        # By default, simulation starts on 1st Jan 2021 
        self.set_clock(datetime(2021,1,1))
        self.actors: list[Actor] = []
        self.cycle = 0

    def set_clock(self, dt: datetime):
        self.sim_clock = dt

    def add_actor(self, a: Actor):
        self.actors.append(a)

    def main_loop(self):
        while True:
            self.cycle += 1
            print('\nMain loop, cycle', self.cycle, ' clock:', self.sim_clock)
            all_events: list[Event] = []
            for a in self.actors:
                if (events := a.set_clock(self.sim_clock)):
                    all_events.extend(events)
            for a in self.actors:
                a.process_events(all_events)
            min_clock = datetime(2000,1,1)
            for a in self.actors:
                if a.next_transition_clock:
                    if a.next_transition_clock>min_clock:
                        min_clock = a.next_transition_clock
            if min_clock == datetime(2000,1,1):
                break
            if sim.sim_clock==min_clock:
                raise ValueError('sim.sim_clock==min_clock')
            sim.set_clock(min_clock)
            
        print('Simulation ends at cycle', self.cycle)


if __name__=='__main__':
    sim = SimMan()
    c1 = Car()
    sim.add_actor(c1)
    sim.main_loop()
