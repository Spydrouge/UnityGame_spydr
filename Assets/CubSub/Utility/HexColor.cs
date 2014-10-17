using System;
using Cubiquity;


//HexColor, a small extension on QuantizedColor

//Purpose: To very quickly and simply allow QuantizedColors to be created with a string of hex characters. 
// Why? Because it is easier and cleaner to type in and edit (more concise), and many designers can think in hex regardless
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{
	/// <summary>
	///<para>A small static extension on QuantizedColor, which otherwise is a Sealed class and likes not to be added on to or  bothered.</para>
	///<para>Purpose: To very quickly and simply allow QuantizedColors to be created with a string of hex characters. </para>
	/// </summary>
		public class HexColor
		{
			/// <summary>
			/// Changes a hex value into a QuantizedColor for use with Cubiquity's Colored Cubes.
			/// </summary>
			/// <returns>A quantized color for use with Cubiquity's Colored Cubes</returns>
			/// <param name="hex">A hex value in string form without any preceeding # or 0x, 8 characters long (rgba) such as "F0F0F0F0" </param>
			public static QuantizedColor QuantHex(String hex)
			{
				byte red = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
				byte green = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
				byte blue = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
				byte alpha = byte.Parse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber);

				return new QuantizedColor(red, green, blue, alpha);
			}

		}
}

