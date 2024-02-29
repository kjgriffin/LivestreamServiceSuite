using System.Collections.Generic;

namespace IntegratedPresenterAPIInterop.DynamicDrivers
{
    public class DynamicControlTButtonDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<DynamicDrawExpression> DrawValues { get; set; } = new List<DynamicDrawExpression>();
        public List<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
    }

    public class DynamicDrawExpression
    {
        public string PKey { get; set; }
        public string Value { get; set; }
        public string VExpr { get; set; }
        public bool IsDynamicValue { get; set; }
        public SumOfProductExpression CondExpr { get; set; }
    }

}
