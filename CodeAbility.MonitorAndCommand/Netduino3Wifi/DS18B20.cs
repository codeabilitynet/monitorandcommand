using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CodeAbility.MonitorAndCommand.Environment
{
    public static class DS18B20
    {
        //Taken from : http://bergonline.dk/post/2013/07/14/howto-Netduino-read-temperature-from-DS18B20.aspx

        public const byte FamilyCode = 0x28;

        // Commands
        public const byte ConvertT = 0x44;
        public const byte CopyScratchpad = 0x48;
        public const byte WriteScratchpad = 0x4E;
        public const byte ReadPowerSupply = 0xB4;
        public const byte RecallE2 = 0xB8;
        public const byte ReadScratchpad = 0xBE;
        public const byte MatchROM = 0x55;
        public const byte SkipROM = 0xCC;

        // Fields
        public static TemperatureFormat TemperatureFormat;

        // Get temperature
        public static float GetTemperature(byte tempLo, byte tempHi)
        {
            float ret = ((short)((tempHi << 8) | tempLo)) / 16F;
            if (TemperatureFormat == TemperatureFormat.Fahrenheit)
            {
                ret = (ret * 9 / 5) + 32;
            }
            return ret;
        }

        // Set device
        public static byte[] SetDevice(OneWire oneWire, object device)
        {
            byte[] b = (byte[])device;
            foreach (var bTmp in b)
            {
                oneWire.TouchByte(bTmp);
            }
            return b;
        }
    }
}
