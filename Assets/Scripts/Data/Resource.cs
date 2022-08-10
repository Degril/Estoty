using System;
using UnityEngine;

namespace Data
{
    //use it for simple project
    [Serializable]
    public class Resource
    {
        public enum Type
        {
            One,
            Two
        }

        public Type type;
        public GameObject prefab;
    }
    
    //use it with odin inspector or serialization from remote json
    
    
    // public abstract class Resource
    // {
    //     public GameObject prefab;
    // }
    //
    // public class ResourceOne : Resource
    // {
    //
    // }
    //
    // public class ResourceTwo : Resource
    // {
    //
    // }
}