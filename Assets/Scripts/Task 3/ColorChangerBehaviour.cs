using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Task_3;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ColorChangerBehaviour : MonoBehaviour
{
    [SerializeField] private MeshRenderer entityPrefab;

    [SerializeField] private int numberOfEntitesInOneAxis;
    [SerializeField] private float animationSpeed = 3;
    [SerializeField] private float animationCooldown = 5;
    [SerializeField] private Color fromColor;
    [SerializeField] private Color toColor;
    
    public int NumberOfEntites => numberOfEntitesInOneAxis * numberOfEntitesInOneAxis * numberOfEntitesInOneAxis;
    
    
    private NativeArray<Vector3> _positions;
    private NativeArray<float> _startTimeToChangeColor; 
    private NativeArray<float> _endTimeToChangeColor; 
    private NativeArray<Color> _colors;
    private float currentTime;
    
    private Material[] _entities; 
    void Start()
    {
        _positions = new NativeArray<Vector3>(NumberOfEntites, Allocator.Persistent);
        _startTimeToChangeColor = new NativeArray<float>(NumberOfEntites, Allocator.Persistent);
        _endTimeToChangeColor = new NativeArray<float>(NumberOfEntites, Allocator.Persistent);
        _colors = new NativeArray<Color>(NumberOfEntites, Allocator.Persistent);
        
        _entities = new Material[NumberOfEntites];

        for (int x = 0, id = 0; x < numberOfEntitesInOneAxis; x++)
        {
            for (int y = 0; y < numberOfEntitesInOneAxis; y++)
            {
                for (int z = 0; z < numberOfEntitesInOneAxis; z++)
                {
                    var entity = Instantiate(entityPrefab, transform);
                    entity.transform.position = new Vector3(x, y, z) * 1.5f;
                    _positions[id] = entity.transform.position;
                    _entities[id++] = entity.material;

                }
            }
        }
        //StaticBatchingUtility.Combine(_entities.Select(entite=> entite.gameObject).ToArray(), gameObject);

        var timeJob = new TimeJob
        {
            startPosition = transform.position,
            animationSpeed = animationSpeed,
            animationStageChangerCooldown = animationCooldown,
            Position = _positions,
            startTimeToChangeColor = _startTimeToChangeColor,
            endTimeToChangeColor = _endTimeToChangeColor,
            
        };
        var timeHandle = timeJob.Schedule(NumberOfEntites, 0);
        timeHandle.Complete();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= animationCooldown * 2)
            currentTime -= animationCooldown * 2;
        
        var colorJob = new ColorJob()
        {
            startTimeToChangeColor = _startTimeToChangeColor,
            endTimeToChangeColor = _endTimeToChangeColor,
            colors = _colors,
            currentTime = currentTime > animationCooldown ? currentTime - animationCooldown : currentTime,
            fromColor = currentTime > animationCooldown ? fromColor : toColor,
            toColor = currentTime > animationCooldown ? toColor : fromColor
        };
        var handle = colorJob.Schedule(NumberOfEntites, 0);
        handle.Complete();
        for (var i = 0; i < NumberOfEntites; i++)
            _entities[i].color = _colors[i];
    }
}
