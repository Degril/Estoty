using System;
using System.Collections.Generic;
using Data.Storages;
using UnityEngine;

namespace Data.ServiceLocator
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, IService> _dictionary = new();

        public static void Add(IService service)
        {
            Debug.Log(service.GetType());
        }

        public static T Get<T>()
        {
            return (T)_dictionary[typeof(T)];
        }
    }
}