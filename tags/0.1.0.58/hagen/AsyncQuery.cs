// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sidi.Persistence;
using System.Web;
using System.Net;

namespace hagen
{
    class CompositeList<T> : IList<T>
    {
        IList<T>[] lists;

        public CompositeList(params IList<T>[] lists)
        {
            this.lists = lists;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                foreach (List<T> list in lists)
                {
                    if (index < list.Count)
                    {
                        return list[index];
                    }
                    else
                    {
                        index -= list.Count;
                    }
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return lists.Sum(x => x.Count); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (IList<T> list in lists)
            {
                foreach (T i in list)
                {
                    yield return i;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (IList<T> list in lists)
            {
                foreach (T i in list)
                {
                    yield return i;
                }
            }
        }

        #endregion
    }
    
    class AsyncQuery
    {
        Collection<Action> actions;
        Sidi.Util.AsyncCalculation<string, IList<Action>> asyncCalculation;
        
        
        public AsyncQuery(Collection<Action> actions)
        {
            this.actions = actions;
            asyncCalculation = new Sidi.Util.AsyncCalculation<string, IList<Action>>(Work);
            asyncCalculation.Complete += new EventHandler(asyncCalculation_Complete);
        }

        void asyncCalculation_Complete(object sender, EventArgs e)
        {
            if (Complete != null)
            {
                Complete(this, EventArgs.Empty);
            }
        }

        public IList<Action> Result
        {
            get
            {
                return asyncCalculation.Result;
            }
        }

        public string Query
        {
            set
            {
                asyncCalculation.Query = value;
            }
        }

        public bool Busy
        {
            get
            {
                return asyncCalculation.Busy;
            }
        }

        public void Refresh()
        {
            asyncCalculation.Query = asyncCalculation.Query;
        }

        IList<Action> Work(string query)
        {
            IList<Action> r = null;

            try
            {
                if (String.IsNullOrEmpty(query) || query.Length <= 2)
                {
                    string sql = String.Format("Name like \"%{0}%\" order by LastUseTime desc limit 20", query);
                    r = actions.Select(sql);
                }
                else
                {
                    string sql = String.Format("Name like \"%{0}%\" order by LastUseTime desc", query);
                    r = actions.Select(sql);

                    List<Action> webLookup = new List<Action>();
                    webLookup.Add(WebLookupAction("Google", "http://www.google.com/search?q={0}", query));
                    webLookup.Add(WebLookupAction("Wikipedia", "http://en.wikipedia.org/wiki/Special:Search?search={0}&go=Go", query));
                    webLookup.Add(WebLookupAction("Leo", "http://dict.leo.org/?lp=ende&search={0}", query));
                    r = new CompositeList<Action>(r, webLookup);
                }
            }
            catch (Exception)
            {
                r = new List<Action>();
            }

            return r;
        }

        public event EventHandler Complete;

        Action WebLookupAction(string title, string urlTemplate, string query)
        {
            Action a = new Action();
            a.Command = String.Format(urlTemplate, HttpUtility.UrlEncode(query));
            a.Name = String.Format("{0} \"{1}\"", title, query);
            return a;
        }
    }
}
