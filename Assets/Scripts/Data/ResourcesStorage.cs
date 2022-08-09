using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class ResourcesStorage : MonoBehaviour
    {
        private Dictionary<Resource.Type, Resource> dictionary = new();
        private void Awake()
        {
            var resources = Resources.Load<ResourcesScriptable>(nameof(ResourcesScriptable));
            foreach (var resource in resources.resources)
            {
                dictionary.Add(resource.type, resource);
            }
        }

        public bool TryGetResource(Resource.Type type, out Resource resource)
        {
            return dictionary.TryGetValue(type, out resource);
        }
    }
}