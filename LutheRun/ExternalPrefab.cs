using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class ExternalPrefab : ExternalElement
    {

        public string TypeIdentifier { get; private set; }

        public string PrefabCommand { get; private set; }
        private bool isPostset = false;
        public int Postset { get; set; } = -1;

        public ExternalPrefab(string command, string typeID)
        {
            TypeIdentifier = typeID;
            PrefabCommand = command;
        }

        public ExternalPrefab(string command, int postset, bool usePostset, string typeID)
        {
            TypeIdentifier = typeID;
            PrefabCommand = command;
            isPostset = usePostset;
            Postset = postset;
        }

        public override string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
            string postset = isPostset ? $"::postset(last={Postset})" : "";
            return $"{PrefabCommand}{postset}";
        }

    }
}
