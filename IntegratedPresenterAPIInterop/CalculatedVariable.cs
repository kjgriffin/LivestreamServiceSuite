using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public class CalculatedVariable
    {
        public string VName { get; set; }
        public AutomationActionArgType VarType { get; set; }
        public object InitVal { get; set; }
        public object LastVal { get; set; }
        public bool IsTracking { get; set; }
        public string VSourcePath { get; set; }
        public string Owner { get; set; }

        public static dynamic ParseDynamicVariableValue(AutomationActionArgType type, dynamic valueSource)
        {
            dynamic result = null;
            switch (type)
            {
                case AutomationActionArgType.Integer:
                    if (valueSource is int)
                        return valueSource;
                    if (valueSource is long)
                        return unchecked((int)(long)valueSource);
                    else if (valueSource is string)
                        return ParseVariableValue(type, (string)valueSource);
                    break;
                case AutomationActionArgType.String:
                    if (valueSource is string)
                        return (string)valueSource;
                    break;
                case AutomationActionArgType.Double:
                    if (valueSource is double)
                        return (double)valueSource;
                    else if (valueSource is string)
                        return ParseVariableValue(type, (string)valueSource);
                    break;
                case AutomationActionArgType.Boolean:
                    if (valueSource is bool)
                        return (bool)valueSource;
                    else if (valueSource is string)
                        return ParseVariableValue(type, (string)valueSource);
                    break;
            }
            return result;
        }
        public static dynamic ParseVariableValue(AutomationActionArgType type, string valueSource)
        {
            dynamic val = null;
            // process equation
            switch (type)
            {
                case AutomationActionArgType.Integer:
                    // yeah.... so int's are 32 bit and longs are 64 bit
                    // so just to be safe do it with longs (even if we describe it as an int)
                    // because some bmd switcher state uses longs
                    if (long.TryParse(valueSource, out long x))
                    {
                        val = (int)x;
                    }
                    break;
                case AutomationActionArgType.String:
                    val = valueSource;
                    break;
                case AutomationActionArgType.Double:
                    if (double.TryParse(valueSource, out double dx))
                    {
                        val = dx;
                    }
                    break;
                case AutomationActionArgType.Boolean:
                    if (bool.TryParse(valueSource, out bool bx))
                    {
                        val = bx;
                    }
                    break;
            }
            return val;
        }
    }


}
