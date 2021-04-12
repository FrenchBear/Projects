# Simulator 2021
# v6, roads and grouped storyboards
# Simulation with actors
# 2021-04-08    PV
# 2021-04-10    PV      Storyboards
# 2021-04-11    PV      MobileActor and retag type MobileActor, Roads and StoryboardGroup

# until Python 3.10: https://stackoverflow.com/questions/33533148/how-do-i-type-hint-a-method-with-the-type-of-the-enclosing-class
from __future__ import annotations
from datetime import datetime, timedelta
from typing import Optional, cast

# =======================================================================================================

class Storyboard:
    def __init__(self, a: Actor, init_state: str, next_state: Optional[str]) -> None:
        self.actor = a
        self.init_state = init_state
        self.next_state = next_state if next_state else 'End_'+init_state

    def start(self) -> None:
        self.actor.state = self.init_state
        self.start_transition_clock = self.actor.act_clock

    def set_clock(self, dt: datetime) -> bool:
        self.t = (dt-self.start_transition_clock).total_seconds()
        self.act_clock = dt
        return False


class StoryboardGroup(Storyboard):
    def __init__(self, l:list[Storyboard], next_state: Optional[str] = None) -> None:
        if len(l)==0: breakpoint()      # Empty list is not supported
        self.list = l
        self.next_state = next_state if next_state else 'End_StoryboardGroup'

    def start(self) -> None:
        self.index = 0
        self.storyboard = self.list[0]
        self.storyboard.next_state = 'End_0'
        self.actor:MobileActor = cast(MobileActor, self.list[0].actor)
        self.storyboard.start()

    def set_clock(self, dt: datetime) -> bool:
        if self.index>=len(self.list): breakpoint()

        res = self.storyboard.set_clock(dt)
        if not res:
            print(f'{dt}: in state {self.actor.state}, dist={self.actor.dist}, speed={self.actor.speed}, accel={self.actor.accel}')
            return False

        self.index += 1
        if self.index>=len(self.list):
            self.actor.state = self.next_state
            return True

        self.storyboard = self.list[self.index]
        if self.actor != self.list[self.index].actor: breakpoint()
        self.storyboard.start()

        print(f'{dt}: transition to state {self.actor.state}, dist={self.actor.dist}, speed={self.actor.speed}, accel={self.actor.accel}')
        return False


class Wait(Storyboard):
    def __init__(self, a: Actor, duration: float, next_state: Optional[str] = None) -> None:
        super().__init__(a, 'Wait', next_state)
        self.tmax = duration

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t < self.tmax:
            return False
        self.actor.state = self.next_state
        return True


class Cruise(Storyboard):
    def __init__(self, a: MobileActor, duration: float, next_state: Optional[str] = None) -> None:
        super().__init__(a, 'Cruise', next_state)
        self.tmax = duration
        self.actor: MobileActor

    def start(self) -> None:
        super().start()
        self.start_dist = self.actor.dist
        self.speed = self.actor.speed
        self.actor.accel = 0

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t <= self.tmax:
            self.dist = self.start_dist + self.speed*self.t
            self.actor.dist = self.dist
            if self.t < self.tmax:
                return False
        if self.t > self.tmax:
            self.dist = self.start_dist + self.speed*self.tmax
            self.actor.dist = self.dist
        self.actor.state = self.next_state
        return True

# Different than math.copysign that returns abs(x)*sign(y)
def adjust_sign(x: float, y: float) -> float:
    return x if y >= 0 else -x


class Acceleration(Storyboard):
    def __init__(self, a: MobileActor, accel: float, target_speed: float, next_state: Optional[str] = None) -> None:
        super().__init__(a, 'Acceleration', next_state)
        self.accel = accel
        self.target_speed = target_speed
        self.actor: MobileActor

    def start(self) -> None:
        if adjust_sign(self.actor.speed, self.accel) > adjust_sign(self.target_speed, self.accel):
            breakpoint()
        super().start()
        self.start_dist = self.actor.dist
        self.start_speed = self.actor.speed
        self.actor.accel = self.accel

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)

        if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
            self.speed = self.start_speed + self.t*self.accel
            #if self.speed<0: self.speed = 0
            self.dist = self.start_dist + self.start_speed*self.t + 0.5*self.accel*self.t**2
            self.actor.speed = self.speed
            self.actor.dist = self.dist
            if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
                return False

        # If current speed exceeds target, compute precise time t where target would be attained
        # and accelerates until this point, then continue up to current time with target speed and
        # no acceleration
        if adjust_sign(self.actor.speed, self.accel) > adjust_sign(self.target_speed, self.accel):
            self.speed = self.target_speed
            t = (self.speed-self.start_speed)/self.accel
            self.dist = self.start_dist + self.start_speed*t + 0.5*self.accel*t**2 + self.speed*(self.t-t)
            self.actor.speed = self.speed
            self.actor.dist = self.dist

        self.actor.state = self.next_state
        return True

# =======================================================================================================


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


class MobileActor(Actor):
    def __init__(self, name: str) -> None:
        super().__init__(name)
        self.dist: float
        self.speed: float
        self.accel: float


# Static class, not an actor
class Road:
    count = 0

    def __init__(self) -> None:
        Road.count += 1
        self.name = f'Road #{Road.count}'
        self.speed_limit = 25   # 25 m/s = 90 km/h, but not used for now


class Car(MobileActor):
    count = 0
    car_accel = 1   # 1_m.s⁻²
    car_decel = 1

    def __init__(self, r: Road) -> None:
        Car.count += 1
        self.road = r
        super().__init__(f'Car #{Car.count}')
        self.start_dist: float = 0
        self.start_speed: float = 0
        self.dist: float = 0
        self.speed: float = 0
        self.accel: float = 0
        self.start_transition_clock: datetime = datetime(2021, 1, 1)
        self.storyboard: Optional[Storyboard] = None

    def set_clock(self, dt: datetime):
        self.act_clock = dt

        if self.storyboard:
            res = self.storyboard.set_clock(dt)
            if res:
                self.storyboard = None
                self.start_transition_clock = dt
            else:
                print(f'{dt}: in state {self.state}, dist={self.dist}, speed={self.speed}, accel={self.accel}, next transition={self.next_transition_clock}')
                return
        else:
            if not self.next_transition_clock:
                return
            t = (dt-self.start_transition_clock).total_seconds()
            if dt < self.next_transition_clock:
                print(f'{dt}: in state {self.state}, dist={self.dist}, speed={self.speed}, accel={self.accel}, next transition={self.next_transition_clock}')
                return

        # State changed, either because storyboard set_clock returned true, or next_transition_clock has been reached
        self.start_transition_clock = dt
        if self.state == 'Init':
            self.storyboard = Wait(self, 2.0)
            self.storyboard.start()
        elif self.state == 'End_Wait':
            self.storyboard = Acceleration(self, accel=Car.car_accel, target_speed=10.0)
            self.storyboard.start()
        elif self.state == 'End_Acceleration':
            self.storyboard = Cruise(self, 30.0)
            self.storyboard.start()
        elif self.state == 'End_Cruise':
            self.storyboard = Acceleration(self, -Car.car_accel, 0.0, next_state='End_Deceleration')
            self.storyboard.start()
        elif self.state == 'End_Deceleration':
            self.next_transition_clock = None
            self.accel = 0
            self.speed = 0
            self.state = 'End'
        print(f'{dt}: transition to state {self.state}, dist={self.dist}, speed={self.speed}, accel={self.accel}, next transition={self.next_transition_clock}')

    def react(self, actors: list[Actor]):
        pass


# =======================================================================================================

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

# =======================================================================================================


if __name__ == '__main__':
    sim = SimMan()
    r1 = Road()
    c1 = Car(r1)
    sim.add_actor(c1)
    sim.main_loop()
