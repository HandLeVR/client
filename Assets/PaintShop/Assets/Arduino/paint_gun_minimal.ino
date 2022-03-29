//inputs
enum sensors {
  Abzug =             A0, //der Abzug halt, selbsterklärend...
  NumberOfTypes
};

byte i = 0;

// init Ports
void setup() {
  for(i=Abzug; i<NumberOfTypes; i++){
    pinMode(i, INPUT);
  }  
  // init serial communication
  Serial.begin(19200);
}

byte j = 0;
int PinValue = 0;
int prevPinValue = 0;
#define avg_sample_count 6
long data[NumberOfTypes] = {0};

//offset correction var'S
#define FIRST_POS          35
#define SECOND_POS         120
#define SCND_POS_RETRACTED 195
#define OFFSET             96
int secondPosExeeded = 0;
int offsetSet = 0;

// main
void loop() {
  prevPinValue = PinValue;
  for(i = Abzug; i < NumberOfTypes; i++){
      //clear old data
      data[i] = 0;
    for(j = 0; j < avg_sample_count; j++){
      //get 6 samples
      data[i] += analogRead(i);
    }
    PinValue = data[i]/avg_sample_count;
    
    //offset correction while release
    if((PinValue > (SECOND_POS + 10)) && (PinValue > SCND_POS_RETRACTED)){
      secondPosExeeded = 1;
      //Serial.println("posExeeded set to 1");
    }
    
    //2nd pos exeeded and handle is beeing released  && (PinValue < prevPinValue)
      //PinValue under 205
      if(secondPosExeeded && (PinValue > SCND_POS_RETRACTED)){
        secondPosExeeded = 0;
        offsetSet = 1;
    }
    
    //if handle released, back to FIRST_POS value
    if((secondPosExeeded == 0) && PinValue < (FIRST_POS + OFFSET)){
      offsetSet = 0;
    }
    //adjusted output
    if(Serial.read() == 'v'){
      if(offsetSet){
        PinValue = PinValue - OFFSET;
        Serial.println(PinValue);
        delay(20);
      }else{
        Serial.println(PinValue); 
        delay(20);
      } 
    }
  }
}
