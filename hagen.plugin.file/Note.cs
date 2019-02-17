// Copyright (c) 2016, Andreas Grimme

namespace hagen
{
    internal class Note
    {
        public string Name;
        public string Content;
        public TextLocation Source;

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Content);
        }
    }
}
