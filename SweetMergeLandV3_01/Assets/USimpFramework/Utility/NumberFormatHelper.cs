using System;
using System.Text;
using UnityEngine;

namespace USimpFramework.Utility
{
    public static class NumberFormatHelper
    {
        public enum ExpressionType
        {
            None = 0,
            Float = 1,
            Int = 2,
            Percent = 3,
        }

        [Serializable]
        public struct NumberFormatSetting
        {
            public ExpressionType expressionType;
            [Range(0, 6)] public int decimalDisplayDigit;
            public bool useDecimalFormatter;
            [Tooltip("If use decimal formatter, the total count of digit (int number) will be clamped to this value")]
            [Range(3, 6)] public int maxIntDisplayDigit;
            [Tooltip("How many digit after comma when converted the value to kilo format")]
            [Range(0, 4)] public int decimalDigitAfterConversion;
            public string floatFormatter;
            public string suffix;
        }

        static StringBuilder strBuilder = new();
        public static float GetExpression(float value, ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Percent:
                    return value * 100;
                case ExpressionType.Int:
                    return Mathf.Round(value); //Todo: decide which method we're going to use to cast value, right we will use round (floor/round/ceil)
                default: break;

            }
            return value;
        }

        


        /// <summary>Format the value for float or int data type, add suffix if need, using round function to cast to int data type/// </summary>
        public static string FormatValue(float value, NumberFormatSetting setting, bool autoExpress = true)
        {
            strBuilder.Clear();
            //Todo: which method we going to cast, (floor/ceil/round), right now we'll use round
            if (autoExpress)
                value = GetExpression(value, setting.expressionType);

            return FloatToSimple(value) + setting.suffix;

            string FloatToSimple(float d)
            {
                if (setting.maxIntDisplayDigit == -1)
                    return string.IsNullOrEmpty(setting.floatFormatter) ? strBuilder.Append(d).ToString() : strBuilder.AppendFormat(setting.floatFormatter, d).ToString();

                int num = (int)Mathf.Pow(10f, setting.maxIntDisplayDigit);//Todo: can optimize or precalculate this

                if (d >= num) //Only thousand number or greater
                {
                    if (d >= 1000000)
                        return DecimalFormat(1000000, "M");
                    else if (d >= 1000)
                        return DecimalFormat(1000, "K");
                }

                return string.IsNullOrEmpty(setting.floatFormatter) ? strBuilder.Append(d).ToString() : strBuilder.AppendFormat(setting.floatFormatter, d).ToString();

                //Kilo must be a divisible by 10^3
                string DecimalFormat(int kilo, string deSuffix)
                {
                    float fNum;
                    int left;
                    var formatter = new StringBuilder();

                    fNum = d / kilo;
                    left = setting.maxIntDisplayDigit - Mathf.FloorToInt(fNum);
                    formatter.Append("{0:0.");
                    for (int i = 0; i < left; i++)
                        formatter.Append('#');
                    formatter.Append('}');
                    strBuilder.AppendFormat(formatter.ToString(), fNum);//toString auto return the rounded value number
                    strBuilder.Append(deSuffix);

                    return strBuilder.ToString();
                }
            }
        }

        /// <summary>Format the value for float or int data type, add suffix if need, using floor function to cast to int data type </summary>
        public static string FormatValue_Floor(float value, NumberFormatSetting setting, bool autoExpression = true)
        {
            strBuilder.Clear();

            if (autoExpression)
                value = GetExpression(value, setting.expressionType);

            var floorNum = MathfUtils.Floor(value, setting.decimalDisplayDigit);
            if (!setting.useDecimalFormatter)
                return string.IsNullOrEmpty(setting.floatFormatter) ? strBuilder.Append(floorNum).Append(setting.suffix).ToString() : strBuilder.AppendFormat(setting.floatFormatter, floorNum).Append(setting.suffix).ToString();

            int num = (int)Mathf.Pow(10f, setting.maxIntDisplayDigit);
            if (value >= num)
            {
                if (value >= 1000000)
                {
                    strBuilder.AppendFormat(setting.floatFormatter, MathfUtils.Floor(value / 1000000, setting.decimalDigitAfterConversion));
                    return strBuilder.Append("M").Append(setting.suffix).ToString();
                }
                if (value >= 1000)
                {
                    strBuilder.AppendFormat(setting.floatFormatter, MathfUtils.Floor(value / 1000, setting.decimalDigitAfterConversion));
                    return strBuilder.Append("K").Append(setting.suffix).ToString();
                }
            }

            return string.IsNullOrEmpty(setting.floatFormatter) ? strBuilder.Append(floorNum).Append(setting.suffix).ToString() : strBuilder.AppendFormat(setting.floatFormatter, floorNum).Append(setting.suffix).ToString();
        }

        /// <summary> Format value (in seconds) in to hour minute seconds format</summary>
        public static string FormatSeconds(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }


        static NumberFormatSetting intNumberKiloFormatSetting = new NumberFormatSetting()
        {
            expressionType = ExpressionType.Int,
            decimalDisplayDigit = 0,
            useDecimalFormatter = true,
            maxIntDisplayDigit = 3,
            decimalDigitAfterConversion = 1,
            floatFormatter = "{0}"
        };
        /// <summary> To kilo format for integer number, adding K, M, B suffix </summary> 
        public static string ToKiloFormat(this int number)
        {
            return FormatValue_Floor(number, intNumberKiloFormatSetting);
        }
    }
}

