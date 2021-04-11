# Check acceleration final adjustment in case final speed exceeds target speed
# 2021-04-11    PV

"""
self_start_dist = 0
self_start_speed = 0
self_target_speed = 9.5
self_t = 10
self_accel = 1.0
"""

self_start_dist = 0
self_start_speed = 9.5
self_target_speed = 0
self_t = 10
self_accel = -1.0

self_speed = self_start_speed + self_t*self_accel
self_dist = self_start_dist + self_start_speed*self_t + 0.5*self_accel*self_t**2
self_actor_speed = self_speed
self_actor_dist = self_dist

# Different than math.copysign that returns abs(x)*sign(y)
def adjust_sign(x:float, y:float) -> float:
    return x if y>=0 else -x

print(f'Step 1: dist={self_actor_dist}, speed={self_actor_speed}')

if adjust_sign(self_actor_speed, self_accel)>adjust_sign(self_target_speed, self_accel):
    self_speed = self_target_speed
    t = (self_speed-self_start_speed)/self_accel
    self_dist = self_start_dist + self_start_speed*t + 0.5*self_accel*t**2 + self_speed*(self_t-t)
    self_actor_speed = self_speed
    self_actor_dist = self_dist
    print(f'Step 2: dist={self_actor_dist}, speed={self_actor_speed}')
