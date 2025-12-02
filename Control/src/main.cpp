#include <Arduino.h>
#include <Adafruit_TinyUSB.h>

// Definición de pines para los botones
const int BTN1 = 2;
const int BTN2 = 3;
const int BTN3 = 4;
const int BTN4 = 5;

// Definición de pines para el joystick
const int JOY_X = 26;  // ADC0 (GPIO 26)
const int JOY_Y = 27;  // ADC1 (GPIO 27)
const int JOY_BTN = 6; // Botón del joystick

// Crear objeto USB HID Gamepad
Adafruit_USBD_HID usb_hid;

// Descriptor de gamepad (8 botones, 2 ejes)
uint8_t const desc_hid_report[] = {
  TUD_HID_REPORT_DESC_GAMEPAD()
};

// Estructura para el reporte del gamepad
hid_gamepad_report_t gp;

void setup() {
  // Configurar botones como entrada con pull-up interno
  pinMode(BTN1, INPUT_PULLUP);
  pinMode(BTN2, INPUT_PULLUP);
  pinMode(BTN3, INPUT_PULLUP);
  pinMode(BTN4, INPUT_PULLUP);
  pinMode(JOY_BTN, INPUT_PULLUP);
  
  // Inicializar USB HID
  usb_hid.setPollInterval(2);
  usb_hid.setReportDescriptor(desc_hid_report, sizeof(desc_hid_report));
  usb_hid.begin();
  
  // Esperar a que USB esté listo
  while(!TinyUSBDevice.mounted()) delay(1);
  
  // Inicializar estructura del gamepad
  gp.x = 0;
  gp.y = 0;
  gp.z = 0;
  gp.rz = 0;
  gp.rx = 0;
  gp.ry = 0;
  gp.hat = 0;
  gp.buttons = 0;
}

void loop() {
  // Verificar que USB esté montado
  if (!TinyUSBDevice.mounted()) {
    delay(10);
    return;
  }
  
  // Leer valores del joystick
  int joyXValue = analogRead(JOY_X);
  int joyYValue = analogRead(JOY_Y);
  
  // Mapear a rango -127 a 127 (formato estándar USB HID)
  gp.x = map(joyXValue, 0, 1023, -127, 127);
  gp.y = map(joyYValue, 0, 1023, -127, 127);
  
  // Aplicar zona muerta
  if (abs(gp.x) < 15) gp.x = 0;
  if (abs(gp.y) < 15) gp.y = 0;
  
  // Leer botones y actualizar el registro de botones
  gp.buttons = 0;
  if (!digitalRead(BTN1)) gp.buttons |= 0x01;  // Botón 1
  if (!digitalRead(BTN2)) gp.buttons |= 0x02;  // Botón 2
  if (!digitalRead(BTN3)) gp.buttons |= 0x04;  // Botón 3
  if (!digitalRead(BTN4)) gp.buttons |= 0x08;  // Botón 4
  if (!digitalRead(JOY_BTN)) gp.buttons |= 0x10;  // Botón 5 (joystick)
  
  // Enviar reporte USB
  usb_hid.sendReport(0, &gp, sizeof(gp));
  
  delay(10); // 100Hz de tasa de refresco
}