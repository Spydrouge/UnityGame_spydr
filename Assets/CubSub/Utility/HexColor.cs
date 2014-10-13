using System;
using Cubiquity;


//HexColor, a small extension on QuantizedColor

//Purpose: To very quickly and simply allow QuantizedColors to be created with a string of hex characters. 
// Why? Because it is easier and cleaner to type in and edit (more concise), and many designers can think in hex regardless
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{
		public class HexColor
		{
			public static QuantizedColor QuantHex(String hex)
			{
				byte red = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
				byte green = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
				byte blue = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
				byte alpha = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);

			return new QuantizedColor(red, green, blue, alpha);
			}

		}
}

