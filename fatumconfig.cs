//   Phloz - Big Data made EZ
//   Copyright (C) 2003-2021 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.Win32;
using FatumCore;
using System.IO.Compression;
using Fatum.FatumCore;

namespace FatumCore
{
    public class fatumconfig
    {
        public static string FatumVersion = "0.9.0 Alpha";
        public string DBDirectory = "c:\\Program Files\\Phloz\\Databases\\";
        public string ServerURL = "http://localhost:7777/";
        public string ConfigDirectory = "c:\\Program Files\\Phloz\\";
        public string ProgramDirectory = "c:\\Program Files\\Phloz\\";
        public string ReportingDirectory = "c:\\Program Files\\Phloz\\Reporting\\";
        public string GraphicsDirectory = "c:\\Program Files\\Phloz\\Graphics\\";
        public string HtmlDirectory = "c:\\Program Files\\Phloz\\html";
        public string TempDirectory = "c:\\Temp";
        public string ServerHost = "master.phloz.com";
        public string ServerMode = "Standalone";
        public string ConnectionString = "";
        public string ServerID = "default";
        public string CloudServer = "cloudmaster.phloz.com";
        public string CloudToken = "123456789";
        public string UniquePhlozIdentifier = "";
        public Boolean RemoteManagement = false;
        public Tree LogDatabases = new Tree();

        //public Tree Configuration = null;

        public int MAXTHREADPOOLSIZE = 8;

        public string CommandLine = "";
        public string PreferenceFile = "";
        public Boolean DEBUG = false;

        public int MaxFileCache = 20000;
        public int MaxSearchCache = 20000;

        public ArrayList AppList = new ArrayList();

        public Boolean MASTER = true;
        public long expiration = 0;
        public int trafficmax = 0;
        public int flowmax = 0;


        public fatumconfig()
        {
            MAXTHREADPOOLSIZE = Environment.ProcessorCount * 8;
        }

        ~fatumconfig()
        {
            FatumVersion = null;
            DBDirectory = null;
            ServerURL = null;
            ConfigDirectory = null;
            ProgramDirectory = null;
            ReportingDirectory = null;
            GraphicsDirectory = null;
            HtmlDirectory = null;
            TempDirectory = null;
            ServerHost = null;
            ServerMode = null;
            ConnectionString = null;
            ServerID = null;
            CloudServer = null;
            CloudToken = null;
            UniquePhlozIdentifier = null;

            if (LogDatabases != null)
            {
                LogDatabases.dispose();
                LogDatabases = null;
            }

            //if (Configuration != null)
            //{
            //    Configuration.dispose();
            //    Configuration = null;
            //}

            CommandLine = null;
            PreferenceFile = null;

            if (AppList != null)
            {
                AppList.Clear();
                AppList = null;
            }
        }

        public Tree loadPreferences()
        {
            Tree result = null;
            string ConfigDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;

            PreferenceFile = ConfigDirectory + "\\settings.xml";

            if (File.Exists(PreferenceFile))
            {
                // Okay, we might be in Development and pointing at some random directory, if so, let's pick the default one and see.
                PreferenceFile = @"C:\Program Files\Phloz\settings.xml";
            }

            if (File.Exists(PreferenceFile))
            {
                result = XMLTree.readXML(PreferenceFile);
                TreeDataAccess.writeXML(PreferenceFile+".bak",result,"Data");

                if (result.getElement("ManagementDatabase")=="")
                {
                    result.setElement("ManagementDatabase", DBDirectory + "\\Phloz");
                }
                if (result.getElement("DocumentDatabaseDirectory") == "")
                {
                    result.setElement("DocumentDatabaseDirectory", DBDirectory + "\\Flows");
                }
                if (result.getElement("ArchiveDirectory") == "")
                {
                    result.setElement("ArchiveDirectory", DBDirectory + "\\Archive");
                }
                if (result.getElement("BackupDirectory") == "")
                {
                    result.setElement("BackupDirectory", DBDirectory + "\\Backups");
                }
                if (result.getElement("StatisticsDirectory") == "")
                {
                    result.setElement("StatisticsDirectory", DBDirectory + "\\Statistics");
                }
                if (result.getElement("ServerMode") == "")
                {
                    result.setElement("ServerMode", "Standalone");
                }
                if (result.getElement("ConnectionString") == "")
                {
                    result.setElement("ConnectionString", "");
                }
                if (result.getElement("ServerHost") == "")
                {
                    result.setElement("ServerHost", "master.phloz.com");
                }
                if (result.getElement("UniqueID") == "")
                {
                    result.setElement("UniqueID", "default");
                }
                if (result.getElement("InstanceName") == "")
                {
                    result.setElement("InstanceName", "default");
                }
                if (result.getElement("CloudServer") == "")
                {
                    result.setElement("CloudServer", "cloudmaster.phloz.com");
                }
                if (result.getElement("CloudToken") == "")
                {
                    result.setElement("CloudToken", "123456789");
                }
                if (result.getElement("RemoteManagement") == "")
                {
                    result.setElement("RemoteManagement", "false");
                }
                if (result.getElement("UniquePhlozIdentifier") == "")
                {
                    result.setElement("UniquePhlozIdentifier", System.Guid.NewGuid().ToString());
                }
                if (result.findNode("LogDatabases") == null)
                {
                    LogDatabases = new Tree();
                    LogDatabases.addElement("Name", "SQLite (Local)");
                    LogDatabases.addElement("Directory", DBDirectory + "\\Flows");
                    LogDatabases.addElement("Type", "SQLite");
                    LogDatabases.addElement("ConnectionString", "None");
                }

                TreeDataAccess.writeXML(PreferenceFile, result, "Data");
            }
            return result;
        }

        //public string GetSetting(string setting)
        //{
        //    string result = Configuration.getElement(setting);
        //    if (result == null) result = "";   
        //    return result;
        //}

        public void dispose()
        {

        }
    }
}

