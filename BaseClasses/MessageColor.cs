//   Flows Libraries -- Flows Common Classes and Methods
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Drawing;
using Proliferation.Fatum;
using System.IO;
using DatabaseAdapters;

namespace Proliferation.Flows
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
                data.AddElement("Category", labelColor.Category);
                data.AddElement("Label", labelColor.Label);
                data.AddElement("HTMLColor", ColorTranslator.ToHtml(labelColor.DocumentColorValue));
                data.AddElement("OwnerID", labelColor.OwnerID);
                data.AddElement("Origin", labelColor.Origin);
                data.AddElement("*@UniqueID", labelColor.UniqueID);
                managementDB.UpdateTree("[Colors]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Colors] ([DateAdded], [Category], [Label], [UniqueID], [HTMLColor], [OwnerID]) VALUES (@DateAdded, @Category, @Label, @UniqueID, @HTMLColor, @OwnerID);";

                Tree NewLabelColor = new Tree();
                NewLabelColor.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewLabelColor.AddElement("@Category", labelColor.Category);
                NewLabelColor.AddElement("@Label", labelColor.Label);
                string uniqueid = "#" + System.Guid.NewGuid().ToString().Replace("-", "");
                labelColor.UniqueID = uniqueid;
                NewLabelColor.AddElement("@UniqueID", uniqueid);
                NewLabelColor.AddElement("@HTMLColor", labelColor.HTMLColor);
                NewLabelColor.AddElement("@OwnerID", labelColor.OwnerID);
                NewLabelColor.AddElement("@Origin", labelColor.Origin);
                managementDB.ExecuteDynamic(sql, NewLabelColor);
                NewLabelColor.Dispose();
            }
        }

        static public void removeColorByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Colors] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void addDocumentColor(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Colors] ([DateAdded], [Category], [Label], [UniqueID], [HTMLColor], [OwnerID]) VALUES (@DateAdded, @Category, @Label, @UniqueID, @HTMLColor, @OwnerID);";

            Tree NewLabelColor = new Tree();
            NewLabelColor.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewLabelColor.AddElement("@Category", description.GetElement("Category"));
            NewLabelColor.AddElement("@Label", description.GetElement("Label"));
            NewLabelColor.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewLabelColor.AddElement("@HTMLColor", description.GetElement("HTMLColor"));
            NewLabelColor.AddElement("@OwnerID", description.GetElement("OwnerID"));
            NewLabelColor.AddElement("@Origin", description.GetElement("Origin"));
            managementDB.ExecuteDynamic(sql, NewLabelColor);
            NewLabelColor.Dispose();
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
            TreeDataAccess.WriteXML(outs, tmp, "DocumentColor");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(DocumentColor current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("Category", current.Category);
            tmp.AddElement("Label", current.Label);
            tmp.AddElement("HTMLColor", current.HTMLColor);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("Origin", current.Origin);
            return tmp;
        }

        static public DocumentColor loadColorByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            String query = "select * from [Colors] where [UniqueID]=@uid;";
            Tree data = new Tree();
            data.AddElement("@uid", uniqueid);
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
