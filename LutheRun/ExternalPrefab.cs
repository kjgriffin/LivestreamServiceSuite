using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class ExternalPrefab : ExternalElement
    {

        public string PrefabCommand { get; private set; }
        private bool isPostset = false;
        public int Postset { get; set; } = -1;

        public ExternalPrefab(string command)
        {
            PrefabCommand = command;
        }

        public ExternalPrefab(string command, int postset)
        {
            PrefabCommand = command;
            isPostset = true;
            Postset = postset;
        }

        public override string XenonAutoGen()
        {
            string postset = isPostset ? $"::postset(last={Postset})" : "";
            return $"{PrefabCommand}{postset}";
        }

    }
}
