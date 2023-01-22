﻿using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Extensions
{
    public static class ListExtensions
    {
        public static bool ContainsAnyArchetype(this IEnumerable<Archetype> list, IEnumerable<Archetype> contained)
        {
            return list.Any(item => contained.Any(containedItem => item == containedItem));
        }
        
        public static bool ContainsAnyArchetype(this IEnumerable<Archetype> list, Archetype contained)
        {
            return list.Any(item =>item == contained);
        }
    }
}