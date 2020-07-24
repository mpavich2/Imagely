using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GroupCStegafy.Utils
{
    /// <summary>
    ///     Defines the StringExtensions class.
    /// </summary>
    public static class StringExtensions
    {
        #region Methods

        /// <summary>
        ///     Removes the non alphabetic characters.
        /// </summary>
        /// <param name="data">The string.</param>
        /// <returns>
        ///     string
        /// </returns>
        public static string RemoveNonAlphabeticCharacters(this string data)
        {
            var regex = new Regex("[^a-zA-Z]");
            return regex.Replace(data, "");
        }

        /// <summary>
        ///     String to a byte array.
        /// </summary>
        /// <param name="data">The hexadecimal.</param>
        /// <returns>
        ///     bit array
        /// </returns>
        public static char[] StringToBitArray(this string data)
        {
            var stringBuilder = new StringBuilder();

            foreach (var currentChar in data.ToCharArray())
            {
                stringBuilder.Append(Convert.ToString(currentChar, 2).PadLeft(8, '0'));
            }

            return stringBuilder.ToString().ToCharArray();
        }

        /// <summary>
        ///     Gets the bytes from binary string.
        /// </summary>
        /// <param name="binary">The binary.</param>
        /// <returns>
        ///     Byte array
        /// </returns>
        public static byte[] GetBytesFromBinaryString(this string binary)
        {
            var bytesAsStrings =
                binary.Select((character, index) => new {Char = character, Index = index})
                      .GroupBy(bit => bit.Index / 8)
                      .Select(grouping => new string(grouping.Select(character => character.Char).ToArray()));
            var bytes = bytesAsStrings.Select(convertedByte => Convert.ToByte(convertedByte, 2)).ToArray();
            return bytes;
        }

        #endregion
    }
}