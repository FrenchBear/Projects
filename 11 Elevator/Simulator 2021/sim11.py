# Simulator 2021
# Simulation with actors and storyboards
# v10, Car react complete
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
from operator import itemgetter

# =======================================================================================================


class Storyboard:
    def __init__(self, a: Actor) -> None:
        self.actor = a
        self.sim: SimMan

    def start(self) -> None:
        self.actor.storyboard = self
        self.start_transition_clock = sim.sim_clock
        print(f'{self.start_transition_clock}: Actor {self.actor.name} starts SB {type(self.actor.storyboard).__name__},', self.actor.status())

    def set_clock(self, dt: datetime) -> bool:
        self.t = (dt-self.start_transition_clock).total_seconds()
        self.act_clock = dt
        return False


class StoryboardGroup(Storyboard):
    def __init__(self, l: list[Storyboard], loop: bool = False) -> None:
        if len(l) == 0:
            breakpoint()      # Empty list is not supported
        super().__init__(l[0].actor)
        self.list = l
        self.loop = loop
        self.index = -1

    def start(self) -> None:
        self.index = 0
        self.storyboard = self.list[0]
        self.actor: MobileActor = cast(MobileActor, self.list[0].actor)
        self.storyboard.start()

    def set_clock(self, dt: datetime) -> bool:
        if self.index < 0:
            return False
        if self.index >= len(self.list):
            breakpoint()

        res = self.storyboard.set_clock(dt)
        if not res:
            return False

        self.index += 1
        if self.loop:
            self.index %= len(self.list)
        if self.index >= len(self.list):
            self.index = -1     # End of storybard group
        else:
            self.storyboard = self.list[self.index]
            if self.actor != self.list[self.index].actor:
                breakpoint()
            self.storyboard.start()

        return False


class Final(Storyboard):
    def __init__(self, a: Actor) -> None:
        super().__init__(a)


class Wait(Storyboard):
    def __init__(self, a: MobileActor, duration: float = 0) -> None:
        super().__init__(a)
        self.duration = duration        # duration=0 -> infinite wait
        self.actor: MobileActor

    # Returns True if sb has ended
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.duration == 0 or self.t < self.duration:
            print(f'{dt}: Actor {self.actor.name} runs SB {type(self.actor.storyboard).__name__},', self.actor.status())
            return False
        print(f'{dt}: Actor {self.actor.name} ends SB {type(self.actor.storyboard).__name__},', self.actor.status())
        self.actor.storyboard = None
        return True

    def start(self) -> None:
        self.actor.accel = 0
        return super().start()

class Cruise(Storyboard):
    def __init__(self, a: MobileActor, duration: float = 0) -> None:
        super().__init__(a)
        self.duration = duration        # durtion==0: no time limit
        self.actor: MobileActor

    def start(self) -> None:
        self.start_position = self.actor.position
        self.speed = self.actor.speed
        self.actor.accel = 0
        super().start()

    # Returns True if SB has ended
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.duration == 0 or self.t <= self.duration:
            self.position = self.start_position + self.speed*self.t
            self.actor.position = self.position
            if self.duration == 0 or self.t < self.duration:
                print(f'{dt}: Actor {self.actor.name} runs SB {type(self.actor.storyboard).__name__},', self.actor.status())
                return False
        if self.t > self.duration:
            self.position = self.start_position + self.speed*self.duration
            self.actor.position = self.position
        print(f'{dt}: Actor {self.actor.name} ends SB {type(self.actor.storyboard).__name__},', self.actor.status())
        self.actor.storyboard = None
        return True

# Different than math.copysign that returns abs(x)*sign(y)


def adjust_sign(x: float, y: float) -> float:
    return x if y >= 0 else -x


class Acceleration(Storyboard):
    def __init__(self, a: MobileActor, accel: float, target_speed: float) -> None:
        super().__init__(a)
        self.accel = accel
        self.target_speed = target_speed
        self.actor: MobileActor

    def start(self) -> None:
        if adjust_sign(self.actor.speed, self.accel) > adjust_sign(self.target_speed, self.accel):
            breakpoint()
        self.start_position = self.actor.position
        self.start_speed = self.actor.speed
        self.actor.accel = self.accel
        super().start()

    # Returns True if SB has ended
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)

        if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
            self.speed = self.start_speed + self.t*self.accel
            #if self.speed<0: self.speed = 0
            self.position = self.start_position + self.start_speed*self.t + 0.5*self.accel*self.t**2
            self.actor.speed = self.speed
            self.actor.position = self.position
            if adjust_sign(self.actor.speed, self.accel) < adjust_sign(self.target_speed, self.accel):
                print(f'{dt}: Actor {self.actor.name} runs SB {type(self.actor.storyboard).__name__},', self.actor.status())
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

        print(f'{dt}: Actor {self.actor.name} ends SB {type(self.actor.storyboard).__name__},', self.actor.status())
        self.actor.storyboard = None
        return True


class SetColor(Storyboard):
    def __init__(self, tl: TrafficLight, road: Road, color: str, duration: float) -> None:
        super().__init__(tl)
        self.road_index = tl.roads.index(road)
        self.color = color
        self.duration = duration
        self.actor: TrafficLight        # More specific type than in super()

    def start(self) -> None:
        self.actor.colors[self.road_index] = self.color
        super().start()

    # Returns True if SB has ended
    def set_clock(self, dt: datetime) -> bool:
        super().set_clock(dt)
        if self.t < self.duration:
            print(f'{dt}: Actor {self.actor.name} runs SB {type(self.actor.storyboard).__name__},', self.actor.status())
            return False
        print(f'{dt}: Actor {self.actor.name} ends SB {type(self.actor.storyboard).__name__},', self.actor.status())
        self.actor.storyboard = None
        return True

# =======================================================================================================


class Actor:
    def __init__(self, name: str) -> None:
        self.name = name
        self.state: str
        self.storyboard: Optional[Storyboard]
        self.sim: SimMan

    # React to all other actors
    def react(self) -> None:
        pass

    def status(self) -> str:
        return ''


class Road(Actor):
    count = 0

    def __init__(self, speed_limit: float) -> None:
        Road.count += 1
        super().__init__(f'Road #{Road.count}')
        self.speed_limit = speed_limit
        self.road_items: dict[float, Actor] = dict()

    def add_item(self, a: Actor, position: float) -> None:
        self.road_items[position] = a


class RoadItem(Actor):
    count = 0

    def __init__(self, road: Road, position: float) -> None:
        RoadItem.count += 1
        super().__init__(f'RoadItem #{CarTarget.count}: {type(self).__name__}')
        self.road = road
        self.position = position
        road.add_item(self, self.position)


class CarTarget(RoadItem):
    def __init__(self, road: Road, position: float) -> None:
        super().__init__(road, position)


class TrafficLight(Actor):
    count = 0

    def __init__(self, road1: Road, road2: Road, pos1: float, pos2: float) -> None:
        TrafficLight.count += 1
        super().__init__(f'Traffic light #{TrafficLight.count} on {road1.name} and {road2.name}')
        self.roads = [road1, road2]
        self.positions = [pos1, pos2]
        self.colors = ['Red', 'Red']       # Init state is all Red
        road1.add_item(self, pos1)
        road2.add_item(self, pos2)

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


def isfloatdiff(f1: float, f2: float) -> bool:
    return abs(f1-f2) > 0.001


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
        # if sim.cycle == 60:
        #     breakpoint()

        # First calculate default accel/speel considering there is no constraint
        target_speed: float = self.road.speed_limit
        target_end_sim: bool = False
        if self.speed < self.road.speed_limit:
            target_accel: float = Car.car_accel
        elif self.speed == self.road.speed_limit:
            target_accel = 0
        else:
            # We're actually speeding, so brake max to get to current limit
            target_accel = Car.car_decel_max

        # Then look for traffic light
        # We care about road items in front of us, up to distance dcut
        # dcut is the distance to decelerate to halt (normal deceleration) if car speed>0
        # if car in Wait state (speed=0), since we target stop 2m before actual traffic light, we take a dcut of 2.5m
        # Maybe a car in wait state should keep a reference to waiting object...
        if self.speed == 0:
            dcut_norm = 2.5
            dcut_min = 0.5
        else:
            dcut_norm = -0.5*self.speed**2/Car.car_decel_norm
            dcut_min = -0.5*self.speed**2/Car.car_decel_max

        for position, actor in sorted(self.road.road_items.items(), key=itemgetter(0)):
            dist = position - 2 - self.position
            if 0 <= dist <= dcut_norm:
                if type(actor) is TrafficLight:
                    tl: TrafficLight = cast(TrafficLight, actor)
                    road_index = tl.roads.index(self.road)
                    if dcut_min <= dist:
                        if tl.colors[road_index] in ['Orange', 'Red']:
                            target_accel_loop: float = -self.speed**2/(2*dist)
                            target_speed_loop: float = 0.0
                            if target_accel_loop < target_accel:
                                target_accel = target_accel_loop
                                target_speed = target_speed_loop
                        # if color is green, target accel/speed calculated by default is good
                    else:
                        # Can't stop before traffic light, so we accelerate if at orange or green
                        if tl.colors[road_index] == 'Red':
                            target_accel = 0
                            target_speed = 0
                        
                elif type(actor) is CarTarget:
                    target_accel_loop = -self.speed**2/(2*dist)
                    target_speed_loop = 0
                    if target_accel_loop < target_accel:
                        target_accel = target_accel_loop
                        target_speed = target_speed_loop
                        target_end_sim = True

        # Ok, now adjust storyboard
        if self.storyboard:
            if target_accel == 0 and target_speed == 0:
                if type(self.storyboard) is not Wait:
                    self.set_storyboard_and_start(Wait(self))
            elif target_accel == 0 and target_speed > 0:
                if type(self.storyboard) is not Cruise:
                    self.set_storyboard_and_start(Cruise(self))
            else:
                reset = False
                if type(self.storyboard) is not Acceleration:
                    reset = True
                else:
                    self.storyboard = cast(Acceleration, self.storyboard)
                    if isfloatdiff(self.storyboard.accel, target_accel) or isfloatdiff(self.storyboard.target_speed, target_speed):
                        reset = True
                if reset:
                    if target_end_sim:
                        nsb = Final(self)
                    elif target_speed == 0:
                        nsb = Wait(self)
                    else:
                        nsb = Cruise(self)
                    self.set_storyboard_and_start(StoryboardGroup(
                        [Acceleration(self, target_accel, target_speed), nsb]))

    def set_storyboard_and_start(self, sb: Storyboard):
        self.sim.storyboards[self] = sb
        sb.start()

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

        c1:Car = self.actors[1]
        tabx = []
        tabp = []
        tabs = []
        taba = []
        while True:
            tabx.append(self.cycle)
            tabp.append(c1.position)
            tabs.append(c1.speed)
            taba.append(c1.accel)
            self.cycle += 1

            # First actors check environment to adjust storybords if needed
            for a in self.actors:
                a.react()
            # Simulation clock advaces one step
            sim.set_clock(sim.sim_clock+timedelta(seconds=1))
            # Then execute storybard time slice
            for s in self.storyboards.values():
                s.set_clock(self.sim_clock)
            # End of simulation when all storboards are in 'End' state
            # if all(s.state == 'End' for s in self.storyboards):
            #    break
            # No, some actors such as traffic light have a looping storyboard and never end
            # For now, simulation ends when all cars are in End state
            if all(a.storyboard and type(a.storyboard) is Final for a in self.actors if type(a) is Car):
                break

        print('Simulation ends at cycle', self.cycle)
        
        import matplotlib.pyplot as plt
        plt.plot(tabx, tabp)
        plt.plot(tabx, tabs)
        plt.plot(tabx, taba)
        plt.show()

# =======================================================================================================


if __name__ == '__main__':
    sim = SimMan()
    sim.add_actor(r1 := Road(10))
    sim.add_actor(c1 := Car(r1))
    c1.set_storyboard_and_start(StoryboardGroup([Acceleration(c1, accel=Car.car_accel, target_speed=r1.speed_limit), Cruise(c1)]))
    # sb1 = StoryboardGroup([ Wait(c1, 2.0),
    #                         Acceleration(c1, accel=Car.car_accel, target_speed=10.0),
    #                         Cruise(c1, 30.0),
    #                         Acceleration(c1, -Car.car_accel, 0.0)],
    #                     'End')
    # sim.add_storyboard(c1, sb1)

    # Objective for car 1
    t1 = CarTarget(r1, 500)

    sim.add_actor(r2 := Road(25))
    sim.add_actor(tl1 := TrafficLight(r1, r2, 150, 250))
    sb2 = StoryboardGroup([SetColor(tl1, r2, 'Green', 30),
                           SetColor(tl1, r2, 'Orange', 3),
                           SetColor(tl1, r2, 'Red', 2),
                           SetColor(tl1, r1, 'Green', 30),
                           SetColor(tl1, r1, 'Orange', 3),
                           SetColor(tl1, r1, 'Red', 2)],
                          loop=True)
    sim.add_storyboard(tl1, sb2)

    sim.main_loop()
