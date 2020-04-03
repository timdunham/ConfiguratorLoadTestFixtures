using System.Linq;
using Newtonsoft.Json.Linq;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class JTokenComparer: IJTokenComparer<JToken>
    {
        public static JTokenComparer Instance = new JTokenComparer();
        //private static Dictionary<string, Func<Tuple<bool, string>, JToken, JToken> > ComparisonFunction = new Dictionary<string, Func<Tuple<bool, string>, JToken, JToken> >
        // private static Dictionary<Type, IJTokenComparer<T> > ComparisonFunction = new Dictionary<Type, IJTokenComparer<T>> 
        // {
        //     { typeof(JValue), new JValueComparer() },
        //     { typeof(JObject), new JObjectComparer() },
        //     { typeof(JArray), new JArrayComparer() },
        // };

        public bool CompareToken(JToken source, JToken target, ref string description)
        {
            if (source==null && target!=null)
            {
                description += $"-source is null but target is {target.ToString()}";
                return false;
            }
            if (source!=null && target ==null)
            {
                description += $"-target is null but source is {target.ToString()}";
                return false;
            }
            if (!(source.GetType()).Equals(target.GetType()))
            {
                description += $"-source and target are different types";
                return false;
            }
            if (source is JObject)
            {
                return new JObjectComparer().CompareToken(source as JObject, target as JObject, ref description);
            }
            else if (source is JArray)
            {
                return new JArrayComparer().CompareToken(source as JArray, target as JArray, ref description);
            }
            else if (source is JValue)
            {
                return new JValueComparer().CompareToken(source as JValue, target as JValue, ref description);
            }
            else if (source is JProperty)
            {
                return new JValueComparer().CompareToken(source as JValue, target as JValue, ref description);
            }
            description += $"unknown type {source.GetType()}";
            return false;
        }
    }
    public interface IJTokenComparer<T>
    {
        bool CompareToken(T source, T target, ref string description);
    }
    internal class JValueComparer : IJTokenComparer<JValue>
    {
        public bool CompareToken(JValue source, JValue target, ref string description)
        {
            if (source.CompareTo(target)!=0)
            {
                description+=$"-{source} doesn't match {target})";
                return false;
            }
            return true;
        }
    }    
    internal class JObjectComparer : IJTokenComparer<JObject>
    {
        public bool CompareToken(JObject source, JObject target, ref string description)
        {
            var sourceInOrder = source.Properties().OrderBy(p=>p.Name);
            var targetInOrder = target.Properties().OrderBy(p=>p.Name);
            foreach (var item in sourceInOrder.Zip(targetInOrder, (s,t)=> (s,t)))
            {
                if (!JTokenComparer.Instance.CompareToken(item.s, item.t, ref description)) 
                {
                    description += $"Prop[{item.s.Name}]"; 
                    return false;
                }
            }
            if (source.Count != target.Count)
            {
                description+= $"source has {source.Count} elements and target has {target.Count} elements";
                return false;
            }
            return true;
        }
    }
    internal class JArrayComparer : IJTokenComparer<JArray>
    {
        public bool CompareToken(JArray source, JArray target, ref string description)
        {
            if (source.Count != target.Count)
            {
                description+= $"source has {source.Count} elements and target has {target.Count} elements";
                return false;
            }
            for (int i = 0; i < source.Count; i++)
            {
                if (!JTokenComparer.Instance.CompareToken(source[i], target[i], ref description))
                {
                    description += $"Array[{i}]";
                    return false;
                }
            }
            return true;
        }
    }
     internal class JPropertyComparer : IJTokenComparer<JProperty>
    {
        public bool CompareToken(JProperty source, JProperty target, ref string description)
        {
            if (source.Name != target.Name)
            {
                description+= $"source has {source.Name} property and target has {target.Name} property";
                return false;
            }
            description += $".{source.Name}";
            return JTokenComparer.Instance.CompareToken(source.Value, target.Value, ref description);
        }
    }
}