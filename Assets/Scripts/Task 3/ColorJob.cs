

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Task_3
{
    public struct ColorJob : IJobParallelFor
    {
        [ReadOnly] public Color fromColor;
        [ReadOnly] public Color toColor;
        [ReadOnly] public float currentTime;
        [ReadOnly] public NativeArray<float> startTimeToChangeColor; 
        [ReadOnly] public NativeArray<float> endTimeToChangeColor; 
        
        [WriteOnly] public NativeArray<Color> colors; 

        public void Execute(int index)
        {
            var startTime = startTimeToChangeColor[index];
            var endTime = endTimeToChangeColor[index];

            if (currentTime < startTime)
                colors[index] = fromColor;
            else if (currentTime > endTime)
                colors[index] = toColor;
            else colors[index] = Color.Lerp(fromColor, toColor, (currentTime - startTime) / (endTime - startTime));
        }
    }
}