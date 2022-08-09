using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = nameof(ResourcesScriptable), menuName = "Data/" + nameof(ResourcesScriptable))]
    public class ResourcesScriptable : ScriptableObject
    {
        public List<Resource> resources;
    }
}