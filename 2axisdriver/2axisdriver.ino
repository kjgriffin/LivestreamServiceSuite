#include <AccelStepper.h>

int initialpos = 0;

AccelStepper stepper1(1, 4, 5);
AccelStepper stepper2(1, 6, 7);


void setup() {
stepper1.setMaxSpeed(2000);
stepper1.setAcceleration(10000);
stepper2.setMaxSpeed(2000);
stepper2.setAcceleration(10000);
}

int val;
int val2;

void loop() {
  // put your main code here, to run repeatedly:

  val = analogRead(A0);
  int pos = stepper1.currentPosition();

  if (val < 400) {
    /*
    if (val < 200) {
      stepper1.moveTo(val);
      stepper1.setSpeed((400 - val) * 2);
    }
    else {
    */
      stepper1.moveTo(val);
      stepper1.setSpeed((400 - val) * 5);
    //}
  }
  else if (val > 600) {
    /*if (val < 800) {
      stepper1.moveTo(-val);
      stepper1.setSpeed((val - 600) * -2);
    }
    else {
    */
      stepper1.moveTo(-val);
      stepper1.setSpeed((val - 600) * -5);
    //}
  }

  val2 = analogRead(A1);
  int pos2 = stepper2.currentPosition();
  if (val2 < 400) {
    /*if (val2 < 200) {
      stepper2.moveTo(val2);
      stepper2.setSpeed((400 - val2) * 2);
    }
    else {
    */
      stepper2.moveTo(val2);
      stepper2.setSpeed((400 - val2) * 5);
    //}
  }
  else if (val2 > 600) {
    /*
    if (val2 < 800) {
      stepper2.moveTo(-val2);
      stepper2.setSpeed((val2 - 600) * -2);
    }
    else {
    */
      stepper2.moveTo(-val2);
      stepper2.setSpeed((val2 - 600) * -5);
    //}
  }

  
  for (int i = 0; i < 100; i++) {
    stepper1.runSpeed();
    stepper2.runSpeed();
  }

}
