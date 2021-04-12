# Simulator 2021
# Simulation with actors and storyboards
# v9, SimMan manages storyboards in a dict
# 2021-04-08    PV
# 2021-04-10    PV      Storyboards
# 2021-04-11    PV      MobileActor and retag type MobileActor, Roads and StoryboardGroup
# 2021-04-12    PV      TrafficLight and SetColor, begir Car.react
#
# until Python 3.10: https://stackoverflow.com/questions/33533148/how-do-i-type-hint-a-method-with-the-type-of-the-enclosing-class
from __future__ import annotations
from datetime import datetime, timedelta
from typing import Optional, cast
import math

# =======================================================================================================

class Storyboard:
    def __init__(self, a: Actor, state: str, next_state: Optional[str] = None) -> None:
        self.actor = a
        self.state = state
        self.next_state = next_state if next_state else 'End_'+state
        self.sim: SimMan

    def start(self) -> None:
        self.actor.state = self.state
        self.start_transition_clock = sim.sim_clock

    def set_clock(self, dt: datetime) -> bool:
        self.t = (dt-self.start_transition_clock).total_seconds()
        self.act_clock = dt
        return False


class StoryboardGroup(Storyboard):
    def __init__(self, l:list[Storyboard], next_state: Optional[str] = None) -> None:
        if len(l)==0: breakpoint()      # Empty list is not supported
        super().__init__(l[0].actor, l[0].state)
        self.list = l
        self.next_state = next_state if next_state else 'End_StoryboardGroup'
        self.index = -1

    def start(self) -> None:
        self.index = 0
        self.storyboard = self.list[0]
        self.storyboard.next_state = 'End_0'
        self.actor:MobileActor = cast(MobileActor, self.list[0].actor)
        self.storyboard.start()     # defines state

    def set_clock(self, dt: datetime) -> bool:
        if self.index<0: return False
        if self.index>=len(self.list): breakpoint()

        res = self.storyboard.set_clock(dt)
        if not res:
            print(f'{dt}: Actor {self.actor.name} in state {self.actor.state},', self.actor.status())
            return False

        self.index += 1
        if self.index>=len(self.list):
            self.state = self.next_state
            self.actor.state = self.next_state
            self.index = -1     # Stopped state
        else:
            self.storyboard = self.list[self.index]
            if self.actor != self.list[self.index].actor: breakpoint()
            self.storyboard.start()

        print(f'{dt}: Actor {self.actor.name} transition to state {self.actor.state},', self.actor.status())
        return False


class Wait(Storyboard):
    def __init__(self, a: Actor, duration: float, next_state: Optional[str] = None) -> None:
        super().__init__(a, 'Wait', next_state)
        self.duration = duration

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t < self.duration:
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
        self.start_position = self.actor.position
        self.speed = self.actor.speed
        self.actor.accel = 0

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t <= self.tmax:
            self.position = self.start_position + self.speed*self.t
            self.actor.position = self.position
            if self.t < self.tmax:
                return False
        if self.t > self.tmax:
            self.position = self.start_position + self.speed*self.tmax
            self.actor.position = self.position
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
        self.start_position = self.actor.position
        self.start_speed = self.actor.speed
        self.actor.accel = self.accel

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)

        if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
            self.speed = self.start_speed + self.t*self.accel
            #if self.speed<0: self.speed = 0
            self.position = self.start_position + self.start_speed*self.t + 0.5*self.accel*self.t**2
            self.actor.speed = self.speed
            self.actor.position = self.position
            if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
                return False

        # If current speed exceeds target, compute precise time t where target would be attained
        # and accelerates until this point, then continue up to current time with target speed and
        # no acceleration
        if adjust_sign(self.actor.speed, self.accel) > adjust_sign(self.target_speed, self.accel):
            self.speed = self.target_speed
            t = (self.speed-self.start_speed)/self.accel
            self.position = self.start_position + self.start_speed*t + 0.5*self.accel*t**2 + self.speed*(self.t-t)
            self.actor.speed = self.speed
            self.actor.position = self.position

        self.actor.state = self.next_state
        return True


class SetColor(Storyboard):
    def __init__(self, tl: TrafficLight, road: Road, color: str, duration: float, next_state: Optional[str] = None) -> None:
        super().__init__(tl, 'SetColor', next_state)
        self.road_index = tl.roads.index(road)
        self.color = color
        self.duration = duration
        self.actor: TrafficLight        # More specific type than in super()

    def start(self) -> None:
        super().start()
        self.actor.colors[self.road_index] = self.color

    # Returns True if state has changed
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t < self.duration:
            return False
        self.actor.state = self.next_state
        return True

# =======================================================================================================


class Actor:
    def __init__(self, name: str) -> None:
        self.name = name
        self.state: str
        self.sim: SimMan

    # React to all other actors
    def react(self) -> None:
        pass

    def status(self) -> str:
        return ''

class Road(Actor):
    count = 0

    def __init__(self) -> None:
        Road.count += 1
        super().__init__(f'Road #{Road.count}')
        self.speed_limit = 25   # 25 m/s = 90 km/h, but not used for now

class TrafficLight(Actor):
    count = 0

    def __init__(self, road1: Road, road2: Road, pos1: float, pos2: float) -> None:
        TrafficLight.count += 1
        super().__init__(f'Traffic light #{TrafficLight.count} on {road1.name} and {road2.name}')
        self.roads = [road1, road2]
        self.positions = [pos1, pos2]
        self.colors = ['Red', 'Red']       # Init state is all Red

    def react(self) -> None:
        pass

    def status(self) -> str:
        return f'color #1={self.colors[0]}, color #2={self.colors[1]}'

class MobileActor(Actor):
    def __init__(self, name: str) -> None:
        super().__init__(name)
        self.position: float
        self.speed: float
        self.accel: float

class Car(MobileActor):
    count = 0
    car_accel = 1   # 1_m.s⁻²
    car_decel_norm = -1
    car_decel_max = -3

    def __init__(self, r: Road) -> None:
        Car.count += 1
        super().__init__(f'Car #{Car.count} on {r.name}')
        self.road = r
        self.start_position: float = 0
        self.start_speed: float = 0
        self.position: float = 0
        self.speed: float = 0
        self.accel: float = 0

    def react(self) -> None:
        for actor in self.sim.actors:
            if type(actor) is TrafficLight:
                tl:TrafficLight = cast(TrafficLight, actor)
                if self.road in tl.roads:
                    road_index = tl.roads.index(self.road)
                    dist = tl.positions[road_index] - self.position
                    if dist>=0 :
                        if tl.colors[road_index] in ['Orange', 'Red']:
                            if self.speed==0: continue
                            dist -= 1       # Stop 1m before traffic light
                            dist_stop_norm = -0.5*self.speed**2/Car.car_decel_norm
                            dist_stop_min = -0.5*self.speed**2/Car.car_decel_max
                            if dist_stop_norm<dist: continue
                            if dist_stop_min>dist: 
                                breakpoint()
                                # ToDo: shold accelerate
                            else:
                                # Decelerate
                                acc = -self.speed**2/(2*dist)

                                # Here we create new sb immediately for testing
                                # In reality, we should consider reaction to all other actors
                                # then select the most stringent one and apply it
                                # We should drop/create only if different from current storyboard
                                self.sim.clear_storyboard(self)
                                sb = StoryboardGroup([Acceleration(self, acc, 0)], 'End')
                                self.sim.add_storyboard(self, sb)
                                sb.start()
                        elif tl.colors[road_index]=='Green':
                            zz = 0
                            # Should accelerate
            elif type(actor) is Car and actor is not self:
                breakpoint()
                pass
                # Check distance with other cars

    def status(self) -> str:
        return f'position={self.position}, speed={self.speed}, accel={self.accel}'


# =======================================================================================================

class SimMan:
    def __init__(self) -> None:
        # By default, simulation starts on 1st Jan 2021
        self.set_clock(datetime(2021, 1, 1))
        self.actors: list[Actor] = []
        self.storyboards: dict[Actor, Storyboard] = {}
        self.cycle = 0

    def set_clock(self, dt: datetime) -> None:
        self.sim_clock = dt

    def add_actor(self, a: Actor) -> None:
        if a in self.actors:
            raise KeyError(f'Actor {a.name} already in actors list')
        a.sim = self
        self.actors.append(a)

    def add_storyboard(self, a: Actor, s: Storyboard) -> None:
        s.sim = self
        if a in self.storyboards:
            raise KeyError(f'Actor {a.name} has already a storybard')
        self.storyboards[a] = s

    def clear_storyboard(self, a: Actor) -> None:
        if a in self.storyboards:
            del self.storyboards[a]

    def main_loop(self) -> None:
        for s in self.storyboards.values():
            s.start()
        while True:
            self.cycle += 1
            # First actors check environment to adjust storybords if needed
            for a in self.actors:
                a.react()
            # Then execute storybard time slice
            for s in self.storyboards.values():
                s.set_clock(self.sim_clock)
            # End of simulation when all storboards are in 'End' state
            if all(s.state == 'End' for s in self.storyboards):
                break
            sim.set_clock(sim.sim_clock+timedelta(seconds=1))

        print('Simulation ends at cycle', self.cycle)

# =======================================================================================================


if __name__ == '__main__':
    sim = SimMan()
    sim.add_actor(r1 := Road())
    sim.add_actor(c1 := Car(r1))
    sb1 = StoryboardGroup([ Wait(c1, 2.0), 
                            Acceleration(c1, accel=Car.car_accel, target_speed=10.0), 
                            Cruise(c1, 30.0), 
                            Acceleration(c1, -Car.car_accel, 0.0)],
                        'End')
    sim.add_storyboard(c1, sb1)

    sim.add_actor(r2 := Road())
    sim.add_actor(tl1 := TrafficLight(r1, r2, 150, 250))
    sb2 = StoryboardGroup([ SetColor(tl1, r2, 'Green', 25),
                            SetColor(tl1, r2, 'Orange', 3),
                            SetColor(tl1, r2, 'Red', 2),
                            SetColor(tl1, r1, 'Green', 25),
                            SetColor(tl1, r1, 'Orange', 3),
                            SetColor(tl1, r1, 'Red', 2)],
                        'End')
    sim.add_storyboard(tl1, sb2)

    sim.main_loop()
