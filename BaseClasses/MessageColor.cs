//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Drawing;
using FatumCore;
using System.IO;
using DatabaseAdapters;

namespace PhlozLib
{
    public class DocumentColor
    {
        public string DateAdded = "";
        public string Category = "";
        public string Label = "";
        public string HTMLColor = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string Origin = "";

        public Color DocumentColorValue = new Color();

        ~DocumentColor()
        {
            DateAdded = null;
            Category = null;
            Label = null;
            HTMLColor = null;
            UniqueID = null;
            OwnerID = null;
            Origin = null;
        }

        static public ArrayList loadColors(CollectionState State)
        {
            return loadColors(State.managementDB);
        }
        
        static public ArrayList loadColors(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Colors];";
            processors = managementDB.Execute(query);

            ArrayList colorList = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                DocumentColor newColor = new DocumentColor();
                newColor.Category = row["Category"].ToString();
                newColor.Label = row["Label"].ToString();
                newColor.HTMLColor = row["HTMLColor"].ToString();
                newColor.UniqueID = row["UniqueID"].ToString();
                newColor.OwnerID = row["OwnerID"].ToString();
                newColor.Origin = row["Origin"].ToString();
                newColor.DocumentColorValue = ColorTranslator.FromHtml(newColor.HTMLColor);
                colorList.Add(newColor);
            }

            return colorList;
        }

        static public void updateColor(DocumentColor labelColor, CollectionState State)
        {
            updateColor(labelColor, State.managementDB);
        }
        static public void updateColor(DocumentColor labelColor, IntDatabase managementDB)
        {
            if (labelColor.HTMLColor == "")
            {
                labelColor.HTMLColor = ColorTranslator.ToHtml(Color.White);
            }
            else
            {
                labelColor.HTMLColor = ColorTranslator.ToHtml(labelColor.DocumentColorValue);
            }

            if (labelColor.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Category", labelColor.Category);
                data.addElement("Label", labelColor.Label);
                data.addElement("HTMLColor", ColorTranslator.ToHtml(labelColor.DocumentColorValue));
                data.addElement("OwnerID", labelColor.OwnerID);
                data.addElement("Origin", labelColor.Origin);
                data.addElement("*@UniqueID", labelColor.UniqueID);
                managementDB.UpdateTree("[Colors]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Colors] ([DateAdded], [Category], [Label], [UniqueID], [HTMLColor], [OwnerID]) VALUES (@DateAdded, @Category, @Label, @UniqueID, @HTMLColor, @OwnerID);";

                Tree NewLabelColor = new Tree();
                NewLabelColor.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewLabelColor.addElement("@Category", labelColor.Category);
                NewLabelColor.addElement("@Label", labelColor.Label);
                string uniqueid = "#" + System.Guid.NewGuid().ToString().Replace("-", "");
                labelColor.UniqueID = uniqueid;
                NewLabelColor.addElement("@UniqueID", uniqueid);
                NewLabelColor.addElement("@HTMLColor", labelColor.HTMLColor);
                NewLabelColor.addElement("@OwnerID", labelColor.OwnerID);
                NewLabelColor.addElement("@Origin", labelColor.Origin);
                managementDB.ExecuteDynamic(sql, NewLabelColor);
                NewLabelColor.dispose();
            }
        }

        static public void removeColorByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Colors] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void addDocumentColor(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Colors] ([DateAdded], [Category], [Label], [UniqueID], [HTMLColor], [OwnerID]) VALUES (@DateAdded, @Category, @Label, @UniqueID, @HTMLColor, @OwnerID);";

            Tree NewLabelColor = new Tree();
            NewLabelColor.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewLabelColor.addElement("@Category", description.getElement("Category"));
            NewLabelColor.addElement("@Label", description.getElement("Label"));
            NewLabelColor.addElement("@UniqueID", description.getElement("UniqueID"));
            NewLabelColor.addElement("@HTMLColor", description.getElement("HTMLColor"));
            NewLabelColor.addElement("@OwnerID", description.getElement("OwnerID"));
            NewLabelColor.addElement("@Origin", description.getElement("Origin"));
            managementDB.ExecuteDynamic(sql, NewLabelColor);
            NewLabelColor.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Colors](" +
                    "[Category] TEXT NULL, " +
                    "[Label] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[HTMLColor] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Colors](" +
                    "[Category] NVARCHAR(100) NULL, " +
                    "[Label] NVARCHAR(100) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[HTMLColor] VARCHAR(16) NULL);";
                    break;

            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basecolors ON Colors([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basecolors ON Colors([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void updateColors(IntDatabase managementDB, ArrayList DocumentColors)
        {
            foreach (DocumentColor current in DocumentColors)
            {
                current.UniqueID = "";
                updateColor(current, managementDB);
            }
        }

        static public string getXML(DocumentColor current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "DocumentColor");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(DocumentColor current)
        {
            Tree tmp = new Tree();
            tmp.addElement("Category", current.Category);
            tmp.addElement("Label", current.Label);
            tmp.addElement("HTMLColor", current.HTMLColor);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("Origin", current.Origin);
            return tmp;
        }

        static public DocumentColor loadColorByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            String query = "select * from [Colors] where [UniqueID]=@uid;";
            Tree data = new Tree();
            data.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, data);

            if (processors.Rows.Count>0)
            {
                DataRow row = processors.Rows[0];
                DocumentColor newColor = new DocumentColor();
                newColor.Category = row["Category"].ToString();
                newColor.Label = row["Label"].ToString();
                newColor.HTMLColor = row["HTMLColor"].ToString();
                newColor.UniqueID = row["UniqueID"].ToString();
                newColor.OwnerID = row["OwnerID"].ToString();
                newColor.Origin = row["Origin"].ToString();
                newColor.DocumentColorValue = ColorTranslator.FromHtml(newColor.HTMLColor);
                return newColor;
            }

            return null;
        }
    }
}
