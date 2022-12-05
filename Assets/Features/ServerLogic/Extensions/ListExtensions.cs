using System;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Extensions
{
    public static class ListExtensions
    {
        public static bool ContainsAnyArchetype(this IList<Archetype> list, IList<Archetype> contained)
        {
            return list.Any(x => contained.Any(c => x == c));
        }
    }
}