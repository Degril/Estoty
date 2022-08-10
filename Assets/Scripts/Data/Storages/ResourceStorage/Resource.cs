using System;
using UnityEngine;

namespace Data
{
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
}