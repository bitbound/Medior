using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Extensions
{
    public static class ObjectExtensions
    {
        public static T2 MapTo<T1, T2>(this T1 self, params string[] exceptProperties)
            where T1 : notnull
            where T2 : new()
        {
            var dataMembers = self.GetType()
                .GetProperties()
                .Where(x => 
                    !exceptProperties.Contains(x.Name) &&
                    x.CustomAttributes.Any(x => x.AttributeType == typeof(DataMemberAttribute)));

            var result = new T2();
            foreach (var member in dataMembers)
            {
                var targetProp = typeof(T2).GetProperty(member.Name);
                if (targetProp is null)
                {
                    continue;
                }

                targetProp.SetValue(result, member.GetValue(self));
            }

            return result;
        }
    }
}
