using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Leblebi.Helper;
public static class EnumHelper
{
    public static string GetDisplayName(Enum value)
    {
        return value.GetType()
                    .GetMember(value.ToString())
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()?
                    .Name ?? value.ToString();
    }
}

