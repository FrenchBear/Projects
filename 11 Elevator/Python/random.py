# Elevator simulation
# random.py - Random numbers generation used by simulation (uniform, Poisson)
# Implemented manually the code can be easily rewritten in other languages
# and produce same results
#
# 2017-09-02 PV

import math


# Values from https://en.wikipedia.org/wiki/Linear_congruential_generator
class UniformIntRandom:
    """Simple linear congruential generator"""
    m = (1 << 31) - 1
    a = 1103515245
    c = 12345

    def __init__(self, seed=0):
        self.seed = seed
        pass

    def max_value(self):
        return UniformIntRandom.m

    def next_value(self):
        self.seed = (UniformIntRandom.a * self.seed + UniformIntRandom.c) % UniformIntRandom.m
        return self.seed

    def next_double(self):
        return self.next_value() / UniformIntRandom.m

    # % max is incorrect for uniform distribution, but impact is low if max << m (acceptable here)
    def next_int(self, max):
        return self.next_value() % max


# From
# https://en.wikipedia.org/wiki/Poisson_distribution#Generating_Poisson-distributed_random_variables
# But finally useless for this simulation, too bad...
class PoissonRandom:
    """Poisson Law Generator (Knuth)"""

    def __init__(self, µ, seed=0):
        self.µ = µ
        self.rnd = UniformIntRandom(seed)

    def next_value(self):
        L = math.exp(-self.µ)
        k = 0
        p = 1
        while True:
            k += 1
            p *= self.rnd.next_double()
            if p <= L: break
        return k - 1


def TestPoisson():
    µ = 15
    f = [0] * 100
    xmax = 0
    poi = PoissonRandom(µ, 0)
    for i in range(1000):
        x = poi.next_value()
        if x < 100: 
            f[x] += 1
            if x > xmax: xmax = x

    for i in range(xmax + 1):
        print('%d\t%d' % (i, f[i]))
