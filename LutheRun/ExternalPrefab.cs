using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun
{
    class ExternalPrefab : ExternalElement
    {

        public string TypeIdentifier { get; private set; }

        public string PrefabCommand { get; private set; }
        private bool isPostset = false;
        private bool hasInjectedPostset = false;
        private int postset = -1;
        public int Postset
        {
            get => postset;
            set
            {
                postset = value;
                if (postset != -1)
                {
                    isPostset = true;
                }
            }
        }

        private string postsetcommand;
        public override string PostsetCmd
        {
            get => postsetcommand;
            set
            {
                postsetcommand = value;
                if (!string.IsNullOrEmpty(value))
                {
                    hasInjectedPostset = true;
                }
            }
        }


        public string PostsetReplacementIdentifier { get; set; } = "";

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
            // Injected postset takes precedence??
            if (hasInjectedPostset)
            {
                postset = PostsetCmd;
            }
            if (string.IsNullOrEmpty(PostsetReplacementIdentifier))
            {
                return $"{PrefabCommand}{postset}";
            }
            else
            {
                string tmp = Regex.Replace(PrefabCommand, Regex.Escape(PostsetReplacementIdentifier), postset);
                return tmp;
            }
        }

    }
}
