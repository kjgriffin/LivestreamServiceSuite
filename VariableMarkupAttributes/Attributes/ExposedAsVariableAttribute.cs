using System.Reflection;

namespace VariableMarkupAttributes.Attributes
{
    public class ExposesWatchableVariablesAttribute : Attribute
    {
    }


    public class ExposedAsVariableAttribute : Attribute
    {
        public string VariablePath { get; private set; }

        public ExposedAsVariableAttribute(string vpath)
        {
            VariablePath = vpath;
        }
    }

    public class ExposedVariable
    {
        public string Path { get; set; }
        public object Value { get; set; }
        public PropertyInfo Metadata { get; set; }
        public AutomationActionArgType TypeInfo { get; set; }
    }


    public static class VariableAttributeFinderHelpers
    {
        public static Dictionary<string, ExposedVariable> FindPropertiesExposedAsVariables(object instance, string path = "")
        {
            string leadingpath = string.IsNullOrEmpty(path) ? "" : path + ".";
            Dictionary<string, ExposedVariable> result = new Dictionary<string, ExposedVariable>();
            foreach (var prop in instance.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<ExposedAsVariableAttribute>();
                if (attr != null)
                {
                    var children = FindPropertiesExposedAsVariables(prop.GetValue(instance), leadingpath + attr.VariablePath);
                    if (children.Any())
                    {
                        foreach (var child in children)
                        {
                            result[child.Key] = child.Value;
                        }
                    }
                    else
                    {
                        // add it it a primitive type...
                        result[leadingpath + attr.VariablePath] = new ExposedVariable
                        {
                            Metadata = prop,
                            Path = attr.VariablePath,
                            Value = prop.GetValue(instance),
                            TypeInfo = ToAutomationType(prop.PropertyType),
                        };
                    }
                }
            }
            return result;
        }

        public static AutomationActionArgType ToAutomationType(Type type)
        {
            if (type == typeof(int) || type == typeof(long))
            {
                return AutomationActionArgType.Integer;
            }
            if (type == typeof(string))
            {
                return AutomationActionArgType.String;
            }
            if (type == typeof(double) || type == typeof(float))
            {
                return AutomationActionArgType.Double;
            }

            return AutomationActionArgType.UNKNOWN_TYPE;
        }

    }
}