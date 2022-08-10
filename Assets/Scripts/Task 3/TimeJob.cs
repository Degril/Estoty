using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Task_3
{
    public struct TimeJob : IJobParallelFor
    {
        [ReadOnly] public Vector3 startPosition;
        [ReadOnly] public float animationSpeed;
        [ReadOnly] public float animationStageChangerCooldown;
        [ReadOnly] public NativeArray<Vector3> Position;
        
        [WriteOnly] public NativeArray<float> startTimeToChangeColor; 
        [WriteOnly] public NativeArray<float> endTimeToChangeColor; 
        public void Execute(int index)
        {
            var distance = Vector3.Distance(startPosition, Position[index]);
            startTimeToChangeColor[index] = distance / animationSpeed;
            endTimeToChangeColor[index] = animationStageChangerCooldown * 0.5f + distance / animationSpeed;
        }
    }
}