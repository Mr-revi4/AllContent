﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Collections;

namespace AllContent_Client
{
    class Favorit
    {
        public string Source { get; private set; }
        public List<ContentUnit> content;
        public int SelectLimit { get; set; }
        private int CurrId;
        List<string> selectResult = null;

        MySqlParameter sqlParamSource;

        public Favorit(string source)
        {
            SelectLimit = 10;
            Source = source;
            content = new List<ContentUnit>();
            sqlParamSource = new MySqlParameter("sour", source);
        }

        public void LoadAll()
        {
            using (DBClient client = new DBClient())
            {
                selectResult = client.SelectQuery("SELECT id, header, description, imgUrl, URL, tags, source, date FROM content " +
                            "WHERE source=@sour" + " ORDER BY id DESC LIMIT " + SelectLimit
                            , sqlParamSource);
                AddToCUList();
            }
        }
        public void Refresh()
        {
            using (DBClient client = new DBClient())
            {
                List<string> chek_id = client.SelectQuery("SELECT MAX(id) FROM content " +
                          "WHERE source = @" + sqlParamSource.ParameterName, sqlParamSource);


                if (Convert.ToUInt32(chek_id[0]) > CurrId)
                {
                    selectResult = client.SelectQuery("SELECT id, header, description, imgUrl, URL, tags, source, date FROM content " +
                            "WHERE source = @" + sqlParamSource.ParameterName + " AND id > " + CurrId.ToString() +
                            " ORDER BY id DESC LIMIT " + SelectLimit, sqlParamSource);
                    AddToCUList();
                }
            }
        }
        private void AddToCUList()
        {
            if (selectResult != null)
            {
                CurrId = (int)Convert.ToUInt32(selectResult[0]);

                for (int i = 0; i < selectResult.Count; i += 8)
                {
                    ContentUnit cu = new ContentUnit();

                    cu.ID = Convert.ToUInt32(selectResult[i]);
                    cu.header = selectResult[i + 1];
                    cu.description = selectResult[i + 2];
                    cu.imgUrl = selectResult[i + 3];
                    cu.URL = selectResult[i + 4];
                    cu.tags = selectResult[i + 5];
                    cu.source = selectResult[i + 6];
                    cu.date = selectResult[i + 7];
                    content.Add(cu);
                }
            }
        }

    }


    class FavoritList : IEnumerable<Favorit>
    {
        public event Action AddEvent = delegate { };
        public event Action DeleteEvent = delegate { };

        private List<Favorit> favorites;
        BackgroundWorker refreshAllContent;

        public FavoritList()
        {
            favorites = new List<Favorit>();
            refreshAllContent = new BackgroundWorker();
            refreshAllContent.DoWork += RefreshAllContent_DoWork;
        }

        private void RefreshAllContent_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var favor in favorites)
            {
                favor.Refresh();

            }
        }


        public void RefreshAll()
        {
            refreshAllContent.RunWorkerAsync();
        }
        public void Add(string source_name, int limit = 10)
        {
            Favorit favor = new Favorit(source_name);
            favor.SelectLimit = limit;
            favor.LoadAll();
            favorites.Add(favor);

            AddEvent();
        }
        public void Delete(string source_name)
        {
            foreach (var favor in favorites)
                if (favor.Source == source_name)
                {
                    favorites.Remove(favor);
                    break;
                }
            DeleteEvent();
        }

        public IEnumerator<Favorit> GetEnumerator()
        {
            return favorites.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return favorites.GetEnumerator();
        }
    }
}