//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatumCore;
using System.IO;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class Config
    {
        Tree currentConfig = new Tree();
        string Filename = "";

        public Config()
        {

        }

        public void LoadConfig(string filename)
        {
            Filename = filename;
            if (File.Exists(filename))
            {
                currentConfig = XMLTree.readXML(filename);
            }
            else
            {
                currentConfig = new Tree();
            }
        }

        public void SaveConfig(string filename)
        {
            TreeDataAccess.writeXML(filename, currentConfig, "Configuration");
        }

        public void SaveConfig()
        {
            TreeDataAccess.writeXML(Filename, currentConfig, "Configuration");
        }

        public void SetConfig(Tree newConfig)
        {
            currentConfig.dispose();
            currentConfig = newConfig;
        }

        public string GetProperty(string property)
        {
            return currentConfig.getElement(property);
        }

        public void SetProperty(string property, string value)
        {
            currentConfig.setElement(property, value);
        }
    }
}
