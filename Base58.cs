using System;
using System.Linq;
using System.Numerics;

namespace ElectricShimmer
{
    public static class Base58
    {
        private const string DIGITS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string Encode(byte[] data)
        {
            try
            {
                // Decode byte[] to BigInteger
                var intData = data.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);

                // Encode BigInteger to Base58 string
                var result = string.Empty;
                while (intData > 0)
                {
                    var remainder = (int)(intData % 58);
                    intData /= 58;
                    result = DIGITS[remainder] + result;
                }

                // Append `1` for each leading 0 byte
                for (var i = 0; i < data.Length && data[i] == 0; i++)
                {
                    result = '1' + result;
                }

                return result;
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
                return null;
            }
        }

        public static byte[] Decode(string data)
        {
            try
            {
                // Decode Base58 string to BigInteger 
                BigInteger intData = 0;
                for (var i = 0; i < data.Length; i++)
                {
                    var digit = DIGITS.IndexOf(data[i]); //Slow

                    if (digit < 0)
                    {
                        throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", data[i], i));
                    }

                    intData = intData * 58 + digit;
                }

                // Encode BigInteger to byte[]
                // Leading zero bytes get encoded as leading `1` characters
                var leadingZeroCount = data.TakeWhile(c => c == '1').Count();
                var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
                var bytesWithoutLeadingZeros =
                  intData.ToByteArray()
                  .Reverse()// to big endian
                  .SkipWhile(b => b == 0);//strip sign byte
                var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

                return result;
            }
            catch (Exception exc)
            {
                Log.Write(exc.Message, LogLevel.EXCEPTION);
                return null;
            }
        }
    }
}
