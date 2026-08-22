
import random


class CicAgent:

    def __init__(self,name):
        self.name = name
        self.distance = 0

    def decide_action(self):

         number = random.randint(0,10001)
         if number <= 4000:
             return "Normal Pace", 70

         elif number <= 6500:
             return "Acceleration", 90

         elif number <= 7500:
             return "Max Eff", 110

         elif number <= 9500:
             return "Fatigue", 50

         elif number <= 9800:
             return "Mech Prob", 10

         else:
             return "Crash", 0


    def act(self):

        momentum , distance = self.decide_action()
        self.distance += distance
        return momentum

    def try_to_arrebasar(self):

        number = random.randint(1,100)
        print(f"{self.name} puede chocar")
        if number <= 50 and len(crashes) > 0 and self.distance < crashes[0] or len(crashes) > 1 and self.distance > crashes[1] :
            return True
        else:
            crashes.append(self.distance)
            return False


Tadej = CicAgent("Tadej")
Isaac = CicAgent("Isaac")
Paul = CicAgent("Paul")


currentCic = [True, True, True]
crashes = []
goal = 170000
actionNum = 0
while( Tadej.distance < goal and Isaac.distance < goal and Paul.distance < goal ):

    #Tadej
    print(f"Action: {actionNum}")
    actionNum += 1

    if(currentCic[0] == True):
        currentMomemtum = Tadej.act()
    elif(currentMomemtum == "Crash"):
        currentCic[0] = Tadej.try_to_arrebasar()

        if( currentCic[0] == True ):
            print(f"Tadej was able to avoid a crash at {Tadej.distance}")
        else:
            print(f"Tadej choco en {Tadej.distance}")


    currentDistance = Tadej.distance
    if (currentCic[0] == True):
        print(f"Tadej sigue, distance: {currentDistance}")

    # Isaac

    if (currentCic[1] == True):
        currentMomemtum = Isaac.act()
    elif(currentMomemtum == "Crash"):
        currentCic[1] = Isaac.try_to_arrebasar()
        if (currentCic[1] == True):
            print(f"Isaac was able to avoid a crash at {Isaac.distance}")
        else:
            print(f"Isaac choco en {Isaac.distance}")

    currentDistance = Isaac.distance
    if (currentCic[1] == True):
        print(f"Isaac sigue, distance: {currentDistance}")


    # Paul

    currentDistance = Paul.distance

    if (currentCic[2] == True):
        currentMomemtum = Paul.act()
    elif (currentMomemtum == "Crash"):
        currentCic[2] = Paul.try_to_arrebasar()
        if (currentCic[2] == True):
            print(f"Paul was able to avoid a crash at {Paul.distance}")
        else:
            print(f"Paul choco en {Paul.distance}")

    if(currentCic[2]==True):
        print(f"Paul sigue, distance: {currentDistance}")

    if(currentCic[0] == False and currentCic[1] == False and currentCic[2] == False):
        break


print("\nAnd The winner is: ")

if(Tadej.distance > Isaac.distance > Paul.distance):
    print(f"Tadej with : {Tadej.distance} meters")
elif(Isaac.distance > Tadej.distance > Paul.distance):
    print(f"Isaac with : {Isaac.distance} meters")
elif(Paul.distance > Tadej.distance > Isaac.distance):
    print(f"Paul with : {Paul.distance} meters")












