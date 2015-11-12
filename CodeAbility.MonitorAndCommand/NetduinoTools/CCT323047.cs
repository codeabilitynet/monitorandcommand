using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware;

namespace Netduino
{
    public class Metrology
    {
        private AnalogInput _currentInput, _voltageInput;
        public Metrology(AnalogInput currentInput, AnalogInput voltageInput)
        {
            _currentInput = currentInput;
            _voltageInput = voltageInput;
        }

        double VCAL, ICAL, PHASECAL;

        public void Calibration(double _VCAL, double _ICAL, double _PHASECAL)
        {
           VCAL = _VCAL;
           ICAL = _ICAL;
           PHASECAL = _PHASECAL;
        }

        private int AnalogRead(AnalogInput inputPin)
        {
            return inputPin.Read();
        }

        private int startV, sampleV, lastSampleV, sampleI, lastSampleI;
        private double filteredV, lastFilteredV, filteredI, lastFilteredI, sqV, sumV, sqI, sumI;
        private double phaseShiftedV;
        private double sumP;
        private double instP;
        private bool lastVCross, checkVCross;
        public double Vrms, Irms, realPower, apparentPower, powerFactor;
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
                startV = AnalogRead(_voltageInput);                    //using the voltage waveform
                if ((startV < 550) && (startV > 440)) st = true;  //check its within range
                crossCount++;
            }

            if (crossCount >= 1000)
            {
                Debug.Print("ERROR:Could not detect zero signal level. Last value=" + startV.ToString());
                return;
            }

            crossCount = 0;
            //-------------------------------------------------------------------------------------------------------------------------
            // 2) Main measurment loop
            //------------------------------------------------------------------------------------------------------------------------- 
            int start = DateTime.Now.Millisecond;    //millis()-start makes sure it doesnt get stuck in the loop if there is an error.

            while ((crossCount < wavelengths) && ((DateTime.Now.Millisecond - start) < timeout))
            {
                numberOfSamples++;                            //Count number of times looped.

                lastSampleV = sampleV;                          //Used for digital high pass filter
                lastSampleI = sampleI;                          //Used for digital high pass filter

                lastFilteredV = filteredV;                    //Used for offset removal
                lastFilteredI = filteredI;                    //Used for offset removal   

                //-----------------------------------------------------------------------------
                // A) Read in raw voltage and current samples
                //-----------------------------------------------------------------------------
                sampleV = AnalogRead(_voltageInput);                 //Read in raw voltage signal
                sampleI = AnalogRead(_currentInput);                 //Read in raw current signal

                //-----------------------------------------------------------------------------
                // B) Apply digital high pass filters to remove 2.5V DC offset (centered on 0V).
                //-----------------------------------------------------------------------------
                filteredV = 0.996 * (lastFilteredV + sampleV - lastSampleV);
                filteredI = 0.996 * (lastFilteredI + sampleI - lastSampleI);

                //-----------------------------------------------------------------------------
                // C) Root-mean-square method voltage
                //-----------------------------------------------------------------------------  
                sqV = filteredV * filteredV;                 //1) square voltage values
                sumV += sqV;                                //2) sum

                //-----------------------------------------------------------------------------
                // D) Root-mean-square method current
                //-----------------------------------------------------------------------------   
                sqI = filteredI * filteredI;                //1) square current values
                sumI += sqI;                                //2) sum 

                //-----------------------------------------------------------------------------
                // E) Phase calibration
                //-----------------------------------------------------------------------------
                phaseShiftedV = lastFilteredV + PHASECAL * (filteredV - lastFilteredV);

                //-----------------------------------------------------------------------------
                // F) Instantaneous power calc
                //-----------------------------------------------------------------------------   
                instP = phaseShiftedV * filteredI;          //Instantaneous Power
                sumP += instP;                               //Sum  

                //-----------------------------------------------------------------------------
                // G) Find the number of times the voltage has crossed the initial voltage
                //    - every 2 crosses we will have sampled 1 wavelength 
                //    - so this method allows us to sample an integer number of wavelengths which increases accuracy
                //-----------------------------------------------------------------------------       
                lastVCross = checkVCross;
                if (sampleV > startV) checkVCross = true;
                else checkVCross = false;
                if (numberOfSamples == 1) lastVCross = checkVCross;

                if (lastVCross != checkVCross) crossCount++;
            }


            //-------------------------------------------------------------------------------------------------------------------------
            // 3) Post loop calculations
            //------------------------------------------------------------------------------------------------------------------------- 
            //Calculation of the root of the mean of the voltage and current squared (rms)
            //Calibration coeficients applied. 
            Vrms = VCAL * sqrt(sumV / numberOfSamples);
            Irms = ICAL * sqrt(sumI / numberOfSamples);

            //Calculation power values
            realPower = VCAL * ICAL * sumP / numberOfSamples;
            apparentPower = Vrms * Irms;
            powerFactor = realPower / apparentPower;

            //--------------------------------------------------
            // kwh increment calculation
            // 1) find out how much time there has been since the last measurement of power
            //--------------------------------------------------
            lwhtime = whtime;
            whtime = DateTime.Now.Ticks;
            whInc = realPower * ((whtime - lwhtime) / 3600000.0);

            //Reset accumulators
            sumV = 0;
            sumI = 0;
            sumP = 0;
            //--------------------------------------------------------------------------------------

            Debug.Print("Vrms = " + Vrms.ToString() + " Irms = " + Irms.ToString() + " Pact = " + apparentPower.ToString() + " Pfct = " + powerFactor.ToString() + " Wh = " + whInc.ToString());
        }

        private double sqrt(double p)
        {
            return System.Math.Pow(p, 0.5);
        }
    }
}
