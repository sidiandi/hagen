using Amg.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amg.Util
{
    public interface IPersonInfo
    {
        Task<Person> GetCurrent();
        Task<Person> GetByMail(string mail);
    }

    public class Person : IEquatable<Person>, IComparable<Person>
    {
        public Person()
        {
        }

        public Dictionary<string, string[]> Properties { get; set; } = new Dictionary<string, string[]>();

        public string DisplayName => String("givenname") + " " + String("sn");
        public string Department => String("department");

        public string Mail => String("mail");

        string String(string key)
        {
            return Properties[key].Join(", ");
        }

        public override string ToString() => DisplayName;

        public bool Equals(Person other)
        {
            return Mail.Equals(other.Mail);
        }

        public override bool Equals(object obj)
        {
            return obj is Person person
                ? Equals(person)
                : false;
        }

        public override int GetHashCode()
        {
            return Mail.GetHashCode();
        }

        public int CompareTo(Person other)
        {
            return this.Mail.CompareTo(other.Mail);
        }
    }
}