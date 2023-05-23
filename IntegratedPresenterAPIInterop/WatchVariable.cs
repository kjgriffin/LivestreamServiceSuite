using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public class WatchVariable
    {

        public WatchVariable(string wpath, object expectation, AutomationActionArgType vType)
        {
            this.VPath = wpath;
            this.ExpectedVal = expectation;
            this.VType = vType;
        }

        public string VPath { get; set; }
        public object ExpectedVal { get; set; }
        public AutomationActionArgType VType { get; set; }
    }


}
