using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
public struct MovableCubeComponent : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;
}
[DisallowMultipleComponent]
[RequiresEntityConversion]
public class MovableCubeComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int playerId;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MovableCubeComponent{PlayerId = playerId});
    }
}