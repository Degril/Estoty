using System;
using Data.Storages;
using UnityEngine;

namespace Data.ServiceLocator
{
    public class ServiceLocatorInitializer : MonoBehaviour
    {
        [SerializeField] private IService[] _services;

        private void Awake()
        {
            foreach (var service in _services)
                ServiceLocator.Add(service);

        }
    }
}