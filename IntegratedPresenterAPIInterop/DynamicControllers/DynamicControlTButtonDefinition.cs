using System.Collections.Generic;

namespace IntegratedPresenterAPIInterop.DynamicDrivers
{
    public class DynamicControlTButtonDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<(string pkey, string value, SumOfProductExpression expr)> DrawValues { get; set; } = new List<(string pkey, string value, SumOfProductExpression expr)>();
        public List<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
    }

}
