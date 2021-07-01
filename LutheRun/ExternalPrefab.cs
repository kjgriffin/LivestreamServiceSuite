using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class ExternalPrefab : ExternalElement
    {
        
        public string PrefabCommand { get; set; }

        public ExternalPrefab(string command)
        {
            PrefabCommand = command;
        }

        public override string XenonAutoGen()
        {
            return $"{PrefabCommand}";
        }

    }
}
