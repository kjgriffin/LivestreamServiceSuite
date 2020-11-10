const int pin_h1 = PD4;
const int pin_h2 = PD5;
const int pin_h3 = PD6;
const int pin_h4 = PD7;
const int pin_v1 = 8;
const int pin_v2 = 9;

void setup() {

  digitalWrite(pin_h1, HIGH);
  digitalWrite(pin_h2, HIGH);
  digitalWrite(pin_h3, HIGH);
  digitalWrite(pin_h4, HIGH);
  digitalWrite(pin_v1, HIGH);
  digitalWrite(pin_v2, HIGH);

  pinMode(pin_h1, OUTPUT);
  pinMode(pin_h2, OUTPUT);
  pinMode(pin_h3, OUTPUT);
  pinMode(pin_h4, OUTPUT);
  pinMode(pin_v1, OUTPUT);
  pinMode(pin_v2, OUTPUT);

  Serial.begin(9600);
}

void loop() {


  if (Serial.available() > 0) {

    int pin = 0;
    int sourceid = Serial.read();
    bool valid = false;

    switch (sourceid)
    {
    case '1':
      pin = pin_h1;
      valid = true;
      break;
    case '2':
      pin = pin_h2;
      valid = true;
      break;
    case '3':
      pin = pin_h3;
      valid = true;
      break;
    case '4':
      pin = pin_h4;
      valid = true;
      break;
    case '5':
      pin = pin_v1;
      valid = true;
      break;
    case '6':
      pin = pin_v2;
      valid = true;
      break;
    
    default:
      break;
    }

    if (valid) {
      digitalWrite(pin, LOW);
      delay(200);
      digitalWrite(pin, HIGH);
    }


  }


}
