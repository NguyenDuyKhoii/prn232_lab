using System.Dynamic;
using System.Reflection;

namespace PRN232.LMS.API.Helpers;

public static class FieldHelper
{
    /// <summary>
    /// Shapes a single object by dynamically extracting only the requested properties.
    /// </summary>
    public static object? ShapeData<T>(T entity, string? fields)
    {
        if (entity == null) return null;
        if (string.IsNullOrWhiteSpace(fields))
        {
            return entity;
        }

        var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(f => f.Trim().ToLower())
                              .ToList();

        if (!fieldList.Any())
        {
            return entity;
        }

        var expandoObject = new ExpandoObject() as IDictionary<string, object?>;
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fieldList)
        {
            // Case-insensitive match on property name
            var property = properties.FirstOrDefault(p => p.Name.ToLower() == field);
            if (property != null)
            {
                var value = property.GetValue(entity);
                expandoObject[property.Name] = value;
            }
        }

        return expandoObject;
    }

    /// <summary>
    /// Shapes a collection of objects by dynamically extracting only the requested properties.
    /// </summary>
    public static List<object?> ShapeDataList<T>(IEnumerable<T> entities, string? fields)
    {
        if (entities == null) return new List<object?>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            return entities.Cast<object?>().ToList();
        }

        return entities.Select(entity => ShapeData(entity, fields)).ToList();
    }
}
