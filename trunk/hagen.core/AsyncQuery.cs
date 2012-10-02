// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Long = Sidi.IO.Long;

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
                foreach (IList<T> list in lists)
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
    
    public class AsyncQuery
    {
        Sidi.Util.AsyncCalculation<string, IList<IAction>> asyncCalculation;
        
        
        public AsyncQuery(IActionSource actionSource)
        {
            this.ActionSource = actionSource;
            asyncCalculation = new Sidi.Util.AsyncCalculation<string, IList<IAction>>(Work);
            asyncCalculation.Complete += new EventHandler(asyncCalculation_Complete);
        }

        void asyncCalculation_Complete(object sender, EventArgs e)
        {
            if (Complete != null)
            {
                Complete(this, EventArgs.Empty);
            }
        }

        public IList<IAction> Result
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

        public IActionSource ActionSource;

        IList<IAction> Work(string query)
        {
            return ActionSource.GetActions(query);
        }

        public event EventHandler Complete;
    }
}
