//inputs
enum sensors {
  Abzug            =  A0, //der Abzug halt, selbsterklärend...
  Strahlregulierer =  A1, //Stellschraube an der Seite -> Einst. für Rund-/Breitstrahlregulierung
  Nadelhub         =  A2, //Stellschraube hinten oben -> Wieviel Farbe soll austreten?
  Luftregulierer   =  A3, //Stellschraube hinten unten -> Luftvolumen anpassen
  NumberOfTypes
};

byte i = 0;

void setup() {
  // init Ports
  for(i=Abzug; i<NumberOfTypes; i++){
    pinMode(i, INPUT);
  }
  // init serial communication
  Serial.begin(19200);
}

byte j = 0;
char input;
#define avg_sample_count 6
long data[NumberOfTypes] = {0};
bool stopSerialPrint = 0;

void loop() {
  //read input
  input = Serial.read();
  //sample data
  for(i = Abzug; i < NumberOfTypes; i++){
      //clear old data
      data[i] = 0;
    for(j = 0; j < avg_sample_count; j++){
      //get 6 samples
      data[i] += analogRead(i);
    }
    //send avg if key input received
    if(input == 'v'){
      Serial.print(data[i]/avg_sample_count); 
      Serial.print(" ");
    }
  }
  if(input == 'v'){
    Serial.println();
  }
}
