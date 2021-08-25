l = ['a','b','c']
print(l.index('b'))

class Actor:
    pass

class RoadItem(Actor):
    pass

class CarTarget(RoadItem):
    pass

ct = CarTarget()
print(type(ct).__name__)
print(ct.__class__.__name__)


d = {1.414: '√2', 3.1416: 'pi'}
for a,b in d.items():
    print(a, b)
    