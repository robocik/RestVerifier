using System.Collections;
using System.Globalization;

namespace RestVerifier.Tests.AspNetCore.ClientAccess;

public static class Helper
{
    public static string ToQueryString(this object request)
    {
        if (request == null)
            throw new ArgumentNullException("request");

        // Get all properties on the object
        var properties = request.GetType().GetProperties()
            .Where(x => x.CanRead)
            .Where(x => x.GetValue(request, null) != null)

            .ToDictionary(x => x.Name, x => x.GetValue(request, null)!);


        // Concat all key/value pairs into a string separated by ampersand
        return string.Join("&", properties
            .SelectMany(x =>
            {
                return ValueToString(x.Key, x.Value);
            }));
    }

    static string[] ValueToString(string name, object value)
    {
        if (value is decimal dec)
        {
            value = dec.ToString(CultureInfo.InvariantCulture);
        }
        if (value is ICollection list)
        {
            var res = new List<string>();
            foreach (var item in list)
            {
                res.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(item.ToString()!)}");
            }

            return res.ToArray();

        }

        if (value is DateTime date)
        {
            value = date.ToString("s");
        }
        return new[]{string.Concat(
                Uri.EscapeDataString(name),
                "=",
                Uri.EscapeDataString(value.ToString()!))};

    }
}