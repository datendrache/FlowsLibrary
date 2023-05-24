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

using Proliferation.Fatum;

namespace Proliferation.Flows.SearchCore
{
    public class SearchThreadDetails
    {
        public string searchterms = "";
        private string SearchWhereClause = "";
        public string LuceneQuerySyntax = "";
        public Tree Terms = null;
        public string label = "";
        public string category = "";
        public string startticks = "";
        public string endticks = "";
        public long searchCount = 0;
        public BaseFlowDB currentDB = null;
        public Tree documents = null;
        public Boolean regex = false;
        public int maxResults = 500;

        //public string running = "true";

        ~SearchThreadDetails()
        {
            searchterms = null;
            SearchWhereClause = null;
            Terms = null;
            label = null;
            category = null;
            startticks = null;
            endticks = null;
            currentDB = null;
            documents = null;
        }
    }
}
