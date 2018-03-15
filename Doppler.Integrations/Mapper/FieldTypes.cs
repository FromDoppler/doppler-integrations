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
        public static string GetDescription(this FieldTypes value)
        {
            var enumType = typeof(FieldTypes);
            var field = enumType.GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? value.ToString();
        }
    }
}