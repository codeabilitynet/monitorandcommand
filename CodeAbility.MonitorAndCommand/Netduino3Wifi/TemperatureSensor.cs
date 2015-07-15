using System;
using System.Threading; 

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using CodeAbility.MonitorAndCommand.MFClient;

namespace CodeAbility.MonitorAndCommand.Netduino3Wifi
{
    public class TemperatureSensor
    {
        // DS18B20, NETMF 4.2
        // center pin is connected to digital pin 0, right pin is connected to 5V,
        // left pin GND, 4k7 pull-up resistor between 5V and the center pin

        // DS18B20
        const byte FamilyCode = 0x28;

        // Commands
        const byte ConvertT = 0x44;
        const byte CopyScratchpad = 0x48;
        const byte WriteScratchpad = 0x4E;
        const byte ReadPowerSupply = 0xB4;
        const byte RecallE2 = 0xB8;
        const byte ReadScratchpad = 0xBE;
        const byte MatchROM = 0x55;
        const byte SkipROM = 0xCC;

        OneWire oneWire;

        public TemperatureSensor(Cpu.Pin pin)
        {
            oneWire = new OneWire(new OutputPort(pin, false));     
        }

        public float ReadTemperature()
        {
            try
            {
                oneWire.TouchReset();

                oneWire.WriteByte(SkipROM); // Skip ROM, we only have one device
                oneWire.WriteByte(ConvertT); // Start temperature conversion
                Thread.Sleep(750);  // Wait Tconv (for default 12-bit resolution)

                oneWire.TouchReset();
                oneWire.WriteByte(SkipROM); // Skip ROM
                oneWire.WriteByte(ReadScratchpad); // Read Scratchpad

                // Read just the temperature (2 bytes)
                var tempLo = oneWire.ReadByte();
                var tempHi = oneWire.ReadByte();
                float temperature = GetTemperature((byte)tempLo, (byte)tempHi);

                return temperature;
            }
            catch(Exception exception)
            {
                Logger.Instance.Write(exception.ToString());
                return 0f;    
            }
        }

        // Get temperature
        float GetTemperature(byte tempLo, byte tempHi)
        {
            float temperature = ((short)((tempHi << 8) | tempLo)) / 16F;
            //Farenheit conversion : ret = (ret * 9 / 5) + 32;            
            return temperature;
        }

        //byte[] SetDevice(OneWire oneWire, object device)
        //{
        //    byte[] b = (byte[])device;
        //    foreach (var bTmp in b)
        //    {
        //        oneWire.TouchByte(bTmp);
        //    }
        //    return b;
        //}
    }
}
