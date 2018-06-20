#include <FirmataPlus86.h>

#define CONNECT_86DUINO_METHOD 0 // 0: USB Serial 1: BlueTooth 2: Arduino WiFi Shield 3: Ethernet 4: ESP8266 WiFi 5: ESP8266 AP

char* projectName = "Hello, 86Duino";
char* _ssid = "your SSID";
char* _password = "your password"; // If it is ESP8266 AP mode, the password can not be shorter that 8 chars.
#define BT_ESP8266_Serial         Serial1
#define BT_ESP8266_Serial_Baud    9600

#if CONNECT_86DUINO_METHOD == 2 // Arduino WiFi Shield
    bool _wep = false;
    IPAddress _ip(0, 0, 0, 0);
#elif CONNECT_86DUINO_METHOD == 3 // Ethernet
    #define ETHERNET_DHCP
    #ifndef ETHERNET_DHCP
        IPAddress localIP(192, 168, 4, 100);
        IPAddress subnet(255, 255, 240, 0);
        IPAddress dnsserver(192, 168, 1, 1);
        IPAddress gateway(192, 168, 1, 1);
    #endif
#elif CONNECT_86DUINO_METHOD == 4 || CONNECT_86DUINO_METHOD == 5 // ESP8266
    int ch_pd_pin = 10;
    uint8_t _chl = 11; // channel ID (only for ESP8266 AP mode)
    uint8_t _ecn = 4; // encryption method (only for ESP8266 AP mode)
#endif
//////////////////////////////////////////////////////////////////////////////////

#include <avr/wdt.h>
// 86ME include lib

// the minimum interval for sampling analog input
#define MINIMUM_SAMPLING_INTERVAL 1

#define REGISTER_NOT_SPECIFIED -1

#define INTER_PING_INTERVAL 40 // 40 ms.

/*==============================================================================
 * GLOBAL VARIABLES
 *============================================================================*/
 

bool checkActiveStart = false;
/* analog inputs */
int analogInputsToReport = 0; // bitwise array to store pin reporting

/* digital input ports */
byte reportPINs[TOTAL_PORTS];       // 1 = report this port, 0 = silence
byte previousPINs[TOTAL_PORTS];     // previous 8 bits sent

/* pins configuration */
byte pinConfig[TOTAL_PINS];         // configuration of every pin
byte portConfigInputs[TOTAL_PORTS]; // each bit: 1 = pin in INPUT, 0 = anything else
int pinState[TOTAL_PINS];           // any value that has been written

/* timer variables */
unsigned long currentMillis;        // store the current value from millis()
unsigned long previousMillis;       // for comparison with currentMillis
unsigned int samplingInterval = 19; // how often to run the main loop (in ms)
unsigned long previousKeepAliveMillis = 0;
unsigned int keepAliveInterval = 0;

boolean isResetting = false;

// test for "w" command to waiting
boolean test_waiting_start = false;
int waiting_id;
int waiting_time;
unsigned long start_waiting_time;

// Ping variables

int numLoops = 0 ;
int pingLoopCounter = 0 ;

uint8_t pingInterval = 33 ;  // Milliseconds between sensor pings (29ms is about the min to avoid
// cross- sensor echo).

#ifdef FIRMATA_SERIAL_FEATURE
SerialFirmata serialFeature;
#endif

// 86ME global variable

/*==============================================================================
 * CLASSES
 *============================================================================*/

// 86ME class

/*==============================================================================
 * NAMESPACES
 *============================================================================*/

// 86ME namespace

/*==============================================================================
 * FUNCTIONS
 *============================================================================*/

// 86ME functions


void outputPort(byte portNumber, byte portValue, byte forceSend)
{
  // pins not configured as INPUT are cleared to zeros
  portValue = portValue & portConfigInputs[portNumber];
  // only send if the value is different than previously sent
  if (forceSend || previousPINs[portNumber] != portValue) {
    Firmata.sendDigitalPort(portNumber, portValue);
    previousPINs[portNumber] = portValue;
  }
}

/* -----------------------------------------------------------------------------
 * check all the active digital inputs for change of state, then add any events
 * to the Serial output queue using Serial.print() */
void checkDigitalInputs(void)
{
  /* Using non-looping code allows constants to be given to readPort().
   * The compiler will apply substantial optimizations if the inputs
   * to readPort() are compile-time constants. */
  if (TOTAL_PORTS > 0 && reportPINs[0]) outputPort(0, readPort(0, portConfigInputs[0]), false);
  if (TOTAL_PORTS > 1 && reportPINs[1]) outputPort(1, readPort(1, portConfigInputs[1]), false);
  if (TOTAL_PORTS > 2 && reportPINs[2]) outputPort(2, readPort(2, portConfigInputs[2]), false);
  if (TOTAL_PORTS > 3 && reportPINs[3]) outputPort(3, readPort(3, portConfigInputs[3]), false);
  if (TOTAL_PORTS > 4 && reportPINs[4]) outputPort(4, readPort(4, portConfigInputs[4]), false);
  if (TOTAL_PORTS > 5 && reportPINs[5]) outputPort(5, readPort(5, portConfigInputs[5]), false);
  if (TOTAL_PORTS > 6 && reportPINs[6]) outputPort(6, readPort(6, portConfigInputs[6]), false);
  if (TOTAL_PORTS > 7 && reportPINs[7]) outputPort(7, readPort(7, portConfigInputs[7]), false);
  if (TOTAL_PORTS > 8 && reportPINs[8]) outputPort(8, readPort(8, portConfigInputs[8]), false);
  if (TOTAL_PORTS > 9 && reportPINs[9]) outputPort(9, readPort(9, portConfigInputs[9]), false);
  if (TOTAL_PORTS > 10 && reportPINs[10]) outputPort(10, readPort(10, portConfigInputs[10]), false);
  if (TOTAL_PORTS > 11 && reportPINs[11]) outputPort(11, readPort(11, portConfigInputs[11]), false);
  if (TOTAL_PORTS > 12 && reportPINs[12]) outputPort(12, readPort(12, portConfigInputs[12]), false);
  if (TOTAL_PORTS > 13 && reportPINs[13]) outputPort(13, readPort(13, portConfigInputs[13]), false);
  if (TOTAL_PORTS > 14 && reportPINs[14]) outputPort(14, readPort(14, portConfigInputs[14]), false);
  if (TOTAL_PORTS > 15 && reportPINs[15]) outputPort(15, readPort(15, portConfigInputs[15]), false);
}

// -----------------------------------------------------------------------------
/* sets the pin mode to the correct state and sets the relevant bits in the
 * two bit-arrays that track Digital I/O and PWM status
 */
void setPinModeCallback(byte pin, int mode)
{
  if (Firmata.getPinMode(pin) == PIN_MODE_IGNORE)
    return;

  if (IS_PIN_ANALOG(pin)) {
    reportAnalogCallback(PIN_TO_ANALOG(pin), mode == PIN_MODE_ANALOG ? 1 : 0); // turn on/off reporting
  }
  if (IS_PIN_DIGITAL(pin)) {
    if (mode == INPUT || mode == PIN_MODE_PULLUP) {
      portConfigInputs[pin / 8] |= (1 << (pin & 7));
    } else {
      portConfigInputs[pin / 8] &= ~(1 << (pin & 7));
    }
  }
  Firmata.setPinState(pin, 0);
  switch (mode) {
    case PIN_MODE_ANALOG:
      if (IS_PIN_ANALOG(pin)) {
        if (IS_PIN_DIGITAL(pin)) {
          pinMode(PIN_TO_DIGITAL(pin), INPUT);    // disable output driver
#if ARDUINO <= 100
          // deprecated since Arduino 1.0.1 - TODO: drop support in Firmata 2.6
          digitalWrite(PIN_TO_DIGITAL(pin), LOW); // disable internal pull-ups
#endif
        }
        Firmata.setPinMode(pin, PIN_MODE_ANALOG);
      }
      break;
    case INPUT:
      if (IS_PIN_DIGITAL(pin)) {
        pinMode(PIN_TO_DIGITAL(pin), INPUT);    // disable output driver
#if ARDUINO <= 100
        // deprecated since Arduino 1.0.1 - TODO: drop support in Firmata 2.6
        digitalWrite(PIN_TO_DIGITAL(pin), LOW); // disable internal pull-ups
#endif
        Firmata.setPinMode(pin, INPUT);
      }
      break;
    case PIN_MODE_PULLUP:
      if (IS_PIN_DIGITAL(pin)) {
        pinMode(PIN_TO_DIGITAL(pin), INPUT_PULLUP);
        Firmata.setPinMode(pin, PIN_MODE_PULLUP);
        Firmata.setPinState(pin, 1);
      }
      break;
    case OUTPUT:
      if (IS_PIN_DIGITAL(pin)) {
        digitalWrite(PIN_TO_DIGITAL(pin), LOW); // disable PWM
        pinMode(PIN_TO_DIGITAL(pin), OUTPUT);
        Firmata.setPinMode(pin, OUTPUT);
      }
      break;
    case PIN_MODE_SERIAL:
#ifdef FIRMATA_SERIAL_FEATURE
      serialFeature.handlePinMode(pin, PIN_MODE_SERIAL);
#endif
      break;
    default:
      Firmata.sendString("Unknown pin mode"); // TODO: put error msgs in EEPROM
      break ;
  }
  // TODO: save status to EEPROM here, if changed
}

/*
 * Sets the value of an individual pin. Useful if you want to set a pin value but
 * are not tracking the digital port state.
 * Can only be used on pins configured as OUTPUT.
 * Cannot be used to enable pull-ups on Digital INPUT pins.
 */
void setPinValueCallback(byte pin, int value)
{
  if (pin < TOTAL_PINS && IS_PIN_DIGITAL(pin)) {
    if (Firmata.getPinMode(pin) == OUTPUT) {
      Firmata.setPinState(pin, value);
      digitalWrite(PIN_TO_DIGITAL(pin), value);
    }
  }
}

void digitalWriteCallback(byte port, int value)
{
  byte pin, lastPin, pinValue, mask = 1, pinWriteMask = 0;

  if (port < TOTAL_PORTS) {
    // create a mask of the pins on this port that are writable.
    lastPin = port * 8 + 8;
    if (lastPin > TOTAL_PINS) lastPin = TOTAL_PINS;
    for (pin = port * 8; pin < lastPin; pin++) {
      // do not disturb non-digital pins (eg, Rx & Tx)
      if (IS_PIN_DIGITAL(pin)) {
        // only write to OUTPUT and INPUT (enables pullup)
        // do not touch pins in PWM, ANALOG, SERVO or other modes
        if (Firmata.getPinMode(pin) == OUTPUT || Firmata.getPinMode(pin) == INPUT) {
          pinValue = ((byte)value & mask) ? 1 : 0;
          if (Firmata.getPinMode(pin) == OUTPUT) {
            pinWriteMask |= mask;
          } else if (Firmata.getPinMode(pin) == INPUT && pinValue == 1 && Firmata.getPinState(pin) != 1) {
            // only handle INPUT here for backwards compatibility
#if ARDUINO > 100
            pinMode(pin, INPUT_PULLUP);
#else
            // only write to the INPUT pin to enable pullups if Arduino v1.0.0 or earlier
            pinWriteMask |= mask;
#endif
          }
          Firmata.setPinState(pin, pinValue);
        }
      }
      mask = mask << 1;
    }
    writePort(port, (byte)value, pinWriteMask);
  }
}


// -----------------------------------------------------------------------------
/* sets bits in a bit array (int) to toggle the reporting of the analogIns
 */
//void FirmataClass::setAnalogPinReporting(byte pin, byte state) {
//}
void reportAnalogCallback(byte analogPin, int value)
{
  if (analogPin < TOTAL_ANALOG_PINS) {
    if (value == 0) {
      analogInputsToReport = analogInputsToReport & ~ (1 << analogPin);
    } else {
      analogInputsToReport = analogInputsToReport | (1 << analogPin);
      // prevent during system reset or all analog pin values will be reported
      // which may report noise for unconnected analog pins
      if (!isResetting) {
        // Send pin value immediately. This is helpful when connected via
        // ethernet, wi-fi or bluetooth so pin states can be known upon
        // reconnecting.
        Firmata.sendAnalog(analogPin, analogRead(analogPin));
      }
    }
  }
  // TODO: save status to EEPROM here, if changed
}

void reportDigitalCallback(byte port, int value)
{
  if (port < TOTAL_PORTS) {
    reportPINs[port] = (byte)value;
    // Send port value immediately. This is helpful when connected via
    // ethernet, wi-fi or bluetooth so pin states can be known upon
    // reconnecting.
    if (value) outputPort(port, readPort(port, portConfigInputs[port]), true);
  }
  // do not disable analog reporting on these 8 pins, to allow some
  // pins used for digital, others analog.  Instead, allow both types
  // of reporting to be enabled, but check if the pin is configured
  // as analog when sampling the analog inputs.  Likewise, while
  // scanning digital pins, portConfigInputs will mask off values from any
  // pins configured as analog
}

/*==============================================================================
 * SYSEX-BASED commands
 *============================================================================*/

void sysexCallback(byte command, byte argc, byte *argv)
{
  byte mode;
  byte stopTX;
  byte slaveAddress;

  byte data;
  int slaveRegister;
  unsigned int delayTime;
  int frequency ;
  int duration ;
  
  long angle;
  long msec;
  
  byte sendBuff[512];
  int i;

  switch (command) {
    
    case KEEP_ALIVE:
      keepAliveInterval = argv[0] + (argv[1] << 7);
      previousKeepAliveMillis = millis();
      break;      
    case SAMPLING_INTERVAL:
      if (argc > 1) {
        samplingInterval = argv[0] + (argv[1] << 7);
        if (samplingInterval < MINIMUM_SAMPLING_INTERVAL) {
          samplingInterval = MINIMUM_SAMPLING_INTERVAL;
        }
        /* calculate number of loops per ping */
        numLoops = INTER_PING_INTERVAL / samplingInterval ;
        //numLoops = 1 ;
      }
      else {
        //Firmata.sendString("Not enough data");
      }
      break;
    case CAPABILITY_QUERY:
      for (i=0; i<sizeof(sendBuff); i++) sendBuff[i] = 0;
      
      sendBuff[0] = START_SYSEX;
      sendBuff[1] = CAPABILITY_RESPONSE;
      i = 2;
      for (byte pin = 0; pin < TOTAL_PINS; pin++) {
        if (IS_PIN_DIGITAL(pin)) {
          sendBuff[i++] = ((byte)INPUT);
          sendBuff[i++] = 1;
          sendBuff[i++] = ((byte)PIN_MODE_PULLUP);
          sendBuff[i++] = 1;
          sendBuff[i++] = ((byte)OUTPUT);
          sendBuff[i++] = 1;
        }
        if (IS_PIN_ANALOG(pin)) {
          sendBuff[i++] = PIN_MODE_ANALOG;
          sendBuff[i++] = 10; // 10 = 10-bit resolution
        }
        if (IS_PIN_PWM(pin)) {
          sendBuff[i++] = PIN_MODE_PWM;
          sendBuff[i++] = DEFAULT_PWM_RESOLUTION;
        }
        if (IS_PIN_DIGITAL(pin)) {
          sendBuff[i++] = PIN_MODE_SERVO;
          sendBuff[i++] = 14;
        }
        if (IS_PIN_I2C(pin)) {
          sendBuff[i++] = PIN_MODE_I2C;
          sendBuff[i++] = 1;  // TODO: could assign a number to map to SCL or SDA
        }
#ifdef FIRMATA_SERIAL_FEATURE
        serialFeature.handleCapability(pin);
#endif
        sendBuff[i++] = 127;
      }
      sendBuff[i++] = END_SYSEX;
      Firmata.write((const uint8_t*)sendBuff, i);
      break;
    case PIN_STATE_QUERY:
      if (argc > 0) {
        for (i=0; i<sizeof(sendBuff); i++) sendBuff[i] = 0;
        
        byte pin = argv[0];
        sendBuff[0] = START_SYSEX;
        sendBuff[1] = PIN_STATE_RESPONSE;
        sendBuff[2] = pin;
        i = 3;
        if (pin < TOTAL_PINS) {
          sendBuff[i++] = Firmata.getPinMode(pin);
          sendBuff[i++] = (byte)Firmata.getPinState(pin) & 0x7F;
          if (Firmata.getPinState(pin) & 0xFF80) sendBuff[i++] = (byte)(Firmata.getPinState(pin) >> 7) & 0x7F;
          if (Firmata.getPinState(pin) & 0xC000) sendBuff[i++] = (byte)(Firmata.getPinState(pin) >> 14) & 0x7F;
        }
        sendBuff[i++] = END_SYSEX;
        Firmata.write((const uint8_t*)sendBuff, i);
      }
      break;
    case ANALOG_MAPPING_QUERY:
      for (i=0; i<sizeof(sendBuff); i++) sendBuff[i] = 0;
      sendBuff[0] = START_SYSEX;
      sendBuff[1] = ANALOG_MAPPING_RESPONSE;
      i = 2;
      for (byte pin = 0; pin < TOTAL_PINS; pin++) {
        sendBuff[i++] = (IS_PIN_ANALOG(pin) ? PIN_TO_ANALOG(pin) : 127);
      }
      sendBuff[i++] = END_SYSEX;
      Firmata.write((const uint8_t*)sendBuff, i);
      break;
    case SERIAL_MESSAGE:
#ifdef FIRMATA_SERIAL_FEATURE
      serialFeature.handleSysex(command, argc, argv);
#endif
      break;
    case CHECK_86DUINO_ACTIVE:
      checkActiveStart = true;
      break;

    // 86ME perform motions case
  }
}

void systemResetCallback()
{
  if (isResetting) return;
  isResetting = true;

  // initialize a defalt state
  // TODO: option to load config from EEPROM instead of default
#ifdef FIRMATA_SERIAL_FEATURE
  serialFeature.reset();
#endif

  for (byte i = 0; i < TOTAL_PORTS; i++) {
    reportPINs[i] = false;    // by default, reporting off
    portConfigInputs[i] = 0;  // until activated
    previousPINs[i] = 0;
  }
  // pins with analog capability default to analog input
  // otherwise, pins default to digital output

  // 86ME reset digital pins
  for (byte i = 0; i < TOTAL_PINS; i++) {
    if (IS_PIN_ANALOG(i)) {
      // turns off pullup, configures everything
      setPinModeCallback(i, PIN_MODE_ANALOG);
    }
  }

  // by default, do not report any analog inputs
  analogInputsToReport = 0;

  /* send digital inputs to set the initial state on the host computer,
   * since once in the loop(), this firmware will only send on change */
  /*
  TODO: this can never execute, since no pins default to digital input
        but it will be needed when/if we support EEPROM stored config
  for (byte i=0; i < TOTAL_PORTS; i++) {
    outputPort(i, readPort(i, portConfigInputs[i]), true);
  }
  */
  isResetting = false;
}

/*==============================================================================
 * SETUP()
 *============================================================================*/

void setup()
{
  #if CONNECT_86DUINO_METHOD == 0
    Firmata.begin(57600);
  #elif CONNECT_86DUINO_METHOD == 1
    Firmata.beginBlueTooth(BT_ESP8266_Serial, BT_ESP8266_Serial_Baud);
  #elif CONNECT_86DUINO_METHOD == 2
    Firmata.beginWiFiShield(projectName, 2000, _ssid, _password, _wep, _ip);
  #elif CONNECT_86DUINO_METHOD == 3
    #ifdef ETHERNET_DHCP
      Firmata.beginEthernet(projectName, 2000);
    #else
      Firmata.beginEthernet(projectName, 2000, localIP, subnet, dnsserver, gateway);
    #endif
  #elif CONNECT_86DUINO_METHOD == 4
      Firmata.beginESP8266(projectName, 2000, BT_ESP8266_Serial, BT_ESP8266_Serial_Baud, ch_pd_pin, _ssid, _password);
  #elif CONNECT_86DUINO_METHOD == 5
      Firmata.beginESP8266_AP(projectName, 2000, BT_ESP8266_Serial, BT_ESP8266_Serial_Baud, ch_pd_pin, _ssid, _password, _chl, _ecn);
  #endif
  
  Firmata.setFirmwareVersion(FIRMATA_FIRMWARE_MAJOR_VERSION, FIRMATA_FIRMWARE_MINOR_VERSION);

  Firmata.attach(DIGITAL_MESSAGE, digitalWriteCallback);
  Firmata.attach(REPORT_ANALOG, reportAnalogCallback);
  Firmata.attach(REPORT_DIGITAL, reportDigitalCallback);
  Firmata.attach(SET_PIN_MODE, setPinModeCallback);
  Firmata.attach(SET_DIGITAL_PIN_VALUE, setPinValueCallback);
  Firmata.attach(START_SYSEX, sysexCallback);
  Firmata.attach(SYSTEM_RESET, systemResetCallback);

  #if CONNECT_86DUINO_METHOD == 2
    pinMode(PIN_TO_DIGITAL(4), OUTPUT);    // switch off SD card bypassing Firmata
    digitalWrite(PIN_TO_DIGITAL(4), HIGH); // SS is active low;
  #endif
  // 86ME setup begin
  systemResetCallback();  // reset to default config
}

void check_localIP()
{
    static bool serial_monitor_open = false;
    if (Serial && serial_monitor_open == false)
    {
        serial_monitor_open = true;
        Serial.print("IP : ");
        Serial.println(Firmata.getLocalIP());
        Serial.print("Gateway : ");
        Serial.println(Firmata.getGatewayIP());
        Serial.print("SubnetMask : ");
        Serial.println(Firmata.getSubnetMask());
    }
    
    if (!Serial && serial_monitor_open == true)
    {
        serial_monitor_open = false;
    }
}

/*==============================================================================
 * LOOP()
 *============================================================================*/
void loop()
{
  byte pin, analogPin;
  int pingResult = 0;

  #if CONNECT_86DUINO_METHOD == 2 || CONNECT_86DUINO_METHOD == 3 || CONNECT_86DUINO_METHOD == 4 || CONNECT_86DUINO_METHOD == 5
    check_localIP();
  #endif
  
  /* DIGITALREAD - as fast as possible, check for changes and output them to the
   * FTDI buffer using Serial.print()  */
  checkDigitalInputs();

  /* STREAMREAD - processing incoming messagse as soon as possible, while still
   * checking digital inputs.  */
  while (Firmata.available())
    Firmata.processInput();

  // TODO - ensure that Stream buffer doesn't go over 60 bytes

  currentMillis = millis();
  if (currentMillis - previousMillis > samplingInterval) {
    previousMillis += samplingInterval;

    // ANALOGREAD - do all analogReads() at the configured sampling interval 
    for (pin = 0; pin < TOTAL_PINS; pin++) {
      if (IS_PIN_ANALOG(pin) && Firmata.getPinMode(pin) == PIN_MODE_ANALOG) {
        analogPin = PIN_TO_ANALOG(pin);
        if (analogInputsToReport & (1 << analogPin)) {
          Firmata.sendAnalog(analogPin, analogRead(analogPin));
        }
      }
    }
    if( keepAliveInterval ) {
       currentMillis = millis();
       if (currentMillis - previousKeepAliveMillis > keepAliveInterval*1000) {
         systemResetCallback();
         
         wdt_enable(WDTO_15MS);
         // systemResetCallback();
         while(1)
            ;
      }
    }
    if (checkActiveStart == true)
    {
	  Firmata.write(START_SYSEX);
	  Firmata.write(_86DUINO_RESPONSE) ;
	  Firmata.write(0x5A)  ;
	  Firmata.write(END_SYSEX);
	  checkActiveStart = false;
    }
  }
}

void printData(char * id,  long data)
{
  char myArray[64] ;

  String myString = String(data);
  myString.toCharArray(myArray, 64) ;
  Firmata.sendString(id) ;
  Firmata.sendString(myArray);
}