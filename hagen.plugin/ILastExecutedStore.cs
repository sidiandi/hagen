using System;

namespace hagen
{
    public interface ILastExecutedStore
    {
        void Set(string id);
        DateTime Get(string id);
    }
}