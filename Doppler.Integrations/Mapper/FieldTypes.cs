using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Doppler.Integrations.Mapper
{
    enum FieldTypes
    {
        [DescriptionAttribute("boolean")]
        Boolean = 1,
        [DescriptionAttribute("string")]
        String = 2,
        [DescriptionAttribute("date")]
        Date = 3
    };

    static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<object>().ToArray();

            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}