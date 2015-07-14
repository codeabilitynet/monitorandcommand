using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace CodeAbility.MonitorAndCommand.Netduino3Wifi
{
    public class TemperatureSensor
    {
        // DS18B20, NETMF 4.2
        // center pin is connected to digital pin 0, right pin is connected to 5V,
        // left pin GND, 4k7 pull-up resistor between 5V and the center pin

        OneWire oneWireSensor;

        public TemperatureSensor()
        {
            oneWireSensor = new OneWire(new OutputPort(Pins.GPIO_PIN_D2, false));     
        }

        public double ReadTemperature()
        {
            //TODO : debug, raises an exception after three calls 

            ushort temperature = 0;

            if (oneWireSensor.TouchReset() > 0)
            {
                oneWireSensor.TouchReset();

                oneWireSensor.WriteByte(0xCC); // Skip ROM, we only have one device

                oneWireSensor.WriteByte(0x44); // Start temperature conversion

                while (oneWireSensor.ReadByte() == 0) ; // wait while busy

                oneWireSensor.TouchReset();

                oneWireSensor.WriteByte(0xCC); // Skip ROM

                oneWireSensor.WriteByte(0xBE); // Read Scratchpad

                temperature = (byte)oneWireSensor.ReadByte(); // LSB

                temperature |= (ushort)(oneWireSensor.ReadByte() << 8); // MSB

                //Temperature in C, If you will in F would be: ((1.80 * (temperature / 16.0)) + 32);
                double convertedTemperature = temperature / 16.0;

                return convertedTemperature;
            }
            else
            {
                return 0d;
            }
        }
    }
}
