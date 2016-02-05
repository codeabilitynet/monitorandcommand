/*
 * Copyright (c) 2015, Paul Gaunard (www.codeability.net)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the 
 *  documentation and/or other materials provided with the distribution.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace CodeAbility.MonitorAndCommand.Windows8Monitor.Converters
{
    public class VoltageToOpacityConverter : IValueConverter
    {
        const double BOARD_REFERENCE_VOLTAGE = 3.3;

        const double LED_MINIMUM_FORWARD_VOLTAGE = 1.5;
        const double LED_MAXIMUM_FORWARD_VOLTAGE = 3.5;

        const double MINIMUM_OPACITY = 0.1;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double result;
            if (value != null && Double.TryParse(value.ToString(), out result))
            {
                return MINIMUM_OPACITY + 0.9 * ((result - LED_MINIMUM_FORWARD_VOLTAGE) / (BOARD_REFERENCE_VOLTAGE - LED_MINIMUM_FORWARD_VOLTAGE));
            }
            else
                return MINIMUM_OPACITY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            //return null; 
            throw new NotImplementedException();
        }
    }
 
}
