using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware;

namespace Netduino
{
    //Adapted from R. Ganesh Metrology.cs posted on Netduino's forum : http://forums.netduino.com/index.php?/topic/1767-energy-meter/

    public class CCT323047
    {
        public double RMSVoltage {get; set; }
        public double RMSCurrent { get; set; }
        public double RealPower { get; set; }
        public double ApparentPower { get; set; }
        public double PowerFactor { get; set; }  

        private AnalogInput currentInput;
        private AnalogInput voltageInput;

        private double calibrationVoltage;
        private double calibrationCurrent;
        private double calibrationPhase;

        public CCT323047(AnalogInput _currentInput, AnalogInput _voltageInput)
        {
            currentInput = _currentInput;
            voltageInput = _voltageInput;
        }

        public void Calibrate(double _calibrationVoltage, double _calibrationCurrent, double _calibrationPhase)
        {
            calibrationVoltage = _calibrationVoltage;
            calibrationCurrent = _calibrationCurrent;
            calibrationPhase = _calibrationPhase;
        }

        private int startVoltage, sampleVoltage, lastSampleVoltage, sampleCurrent, lastSampleCurrent;
        private double filteredVoltage, lastFilteredVoltage, filteredCurrent, lastFilteredCurrent;
        private double phaseShiftedVoltage;
        private double squaredVoltage, squaredCurrent; 
        private double voltageSum, currentSum, powerSum;
        private double instantaneousPower;
        private bool lastVoltageCross, checkVoltageCross;
        private long lwhtime, whtime;
        private double whInc;

        public void Measure(int wavelengths, int timeout)
        {
            int crossCount = 0;                             //Used to measure number of times threshold is crossed.
            int numberOfSamples = 0;                        //This is now incremented  

            //-------------------------------------------------------------------------------------------------------------------------
            // 1) Waits for the waveform to be close to 'zero' (500 adc) part in sin curve.
            //-------------------------------------------------------------------------------------------------------------------------
            bool st = false;                                  //an indicator to exit the while loop
            while (st == false && crossCount < 1000)                                   //the while loop...
            {
                startVoltage = AnalogRead(voltageInput);                    //using the voltage waveform
                if ((startVoltage < 550) && (startVoltage > 440)) st = true;  //check its within range
                crossCount++;
            }

            if (crossCount >= 1000)
            {
                Debug.Print("ERROR:Could not detect zero signal level. Last value=" + startVoltage.ToString());
                return;
            }

            crossCount = 0;
            //-------------------------------------------------------------------------------------------------------------------------
            // 2) Main measurment loop
            //------------------------------------------------------------------------------------------------------------------------- 
            int start = DateTime.Now.Millisecond;    //millis()-start makes sure it doesnt get stuck in the loop if there is an error.

            while ((crossCount < wavelengths) && ((DateTime.Now.Millisecond - start) < timeout))
            {
                numberOfSamples++; //Count number of times looped.

                lastSampleVoltage = sampleVoltage; //Used for digital high pass filter
                lastSampleCurrent = sampleCurrent; //Used for digital high pass filter

                lastFilteredVoltage = filteredVoltage; //Used for offset removal
                lastFilteredCurrent = filteredCurrent; //Used for offset removal   

                // A) Read in raw voltage and current samples
                sampleVoltage = AnalogRead(voltageInput); //Read in raw voltage signal
                sampleCurrent = AnalogRead(currentInput); //Read in raw current signal

                // B) Apply digital high pass filters to remove 2.5V DC offset (centered on 0V).
                filteredVoltage = 0.996 * (lastFilteredVoltage + sampleVoltage - lastSampleVoltage);
                filteredCurrent = 0.996 * (lastFilteredCurrent + sampleCurrent - lastSampleCurrent);

                // C) Root-mean-square method voltage
                squaredVoltage = filteredVoltage * filteredVoltage; //1) square voltage values
                voltageSum += squaredVoltage; //2) sum

                // D) Root-mean-square method current
                squaredCurrent = filteredCurrent * filteredCurrent; //1) square current values
                currentSum += squaredCurrent; //2) sum 

                // E) Phase calibration
                phaseShiftedVoltage = lastFilteredVoltage + calibrationPhase * (filteredVoltage - lastFilteredVoltage);

                // F) Instantaneous power calc
                instantaneousPower = phaseShiftedVoltage * filteredCurrent; //Instantaneous Power
                powerSum += instantaneousPower; //Sum  

                // G) Find the number of times the voltage has crossed the initial voltage
                //    - every 2 crosses we will have sampled 1 wavelength 
                //    - so this method allows us to sample an integer number of wavelengths which increases accuracy
                lastVoltageCross = checkVoltageCross;
                if (sampleVoltage > startVoltage) checkVoltageCross = true;
                else checkVoltageCross = false;
                if (numberOfSamples == 1) lastVoltageCross = checkVoltageCross;

                if (lastVoltageCross != checkVoltageCross) crossCount++;
            }

            // 3) Post loop calculations
            //Calculation of the root of the mean of the voltage and current squared (rms)
            //Calibration coeficients applied. 
            RMSVoltage = calibrationVoltage * SquaredRoot(voltageSum / numberOfSamples);
            RMSCurrent = calibrationCurrent * SquaredRoot(currentSum / numberOfSamples);

            //Calculation power values
            RealPower = calibrationVoltage * calibrationCurrent * powerSum / numberOfSamples;
            ApparentPower = RMSVoltage * RMSCurrent;
            PowerFactor = RealPower / ApparentPower;

            // kwh increment calculation
            // 1) find out how much time there has been since the last measurement of power
            lwhtime = whtime;
            whtime = DateTime.Now.Ticks;
            whInc = RealPower * ((whtime - lwhtime) / 3600000.0);

            //Reset accumulators
            voltageSum = 0;
            currentSum = 0;
            powerSum = 0;
            //--------------------------------------------------------------------------------------

            Debug.Print("rmsVoltage = " + RMSVoltage.ToString() + " rmsCurrent = " + RMSCurrent.ToString() + " apparentPower = " + ApparentPower.ToString() + " PowerFactor = " + PowerFactor.ToString() + " Wh = " + whInc.ToString());
        }

        private int AnalogRead(AnalogInput inputPin)
        {
            return inputPin.Read();
        }

        private double SquaredRoot(double p)
        {
            return System.Math.Pow(p, 0.5);
        }
    }
}
