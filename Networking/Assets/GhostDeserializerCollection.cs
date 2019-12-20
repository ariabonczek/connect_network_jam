using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct NetworkingGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "CubeGhostSerializer",
            "SphereGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curCubeGhostSpawnSystem = world.GetOrCreateSystem<CubeGhostSpawnSystem>();
        m_CubeSnapshotDataNewGhostIds = curCubeGhostSpawnSystem.NewGhostIds;
        m_CubeSnapshotDataNewGhosts = curCubeGhostSpawnSystem.NewGhosts;
        curCubeGhostSpawnSystem.GhostType = 0;
        var curSphereGhostSpawnSystem = world.GetOrCreateSystem<SphereGhostSpawnSystem>();
        m_SphereSnapshotDataNewGhostIds = curSphereGhostSpawnSystem.NewGhostIds;
        m_SphereSnapshotDataNewGhosts = curSphereGhostSpawnSystem.NewGhosts;
        curSphereGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_CubeSnapshotDataFromEntity = system.GetBufferFromEntity<CubeSnapshotData>();
        m_SphereSnapshotDataFromEntity = system.GetBufferFromEntity<SphereSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        DataStreamReader reader,
        ref DataStreamReader.Context ctx, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<NetworkingGhostDeserializerCollection>.InvokeDeserialize(m_CubeSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, reader, ref ctx, compressionModel);
            case 1:
                return GhostReceiveSystem<NetworkingGhostDeserializerCollection>.InvokeDeserialize(m_SphereSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, reader, ref ctx, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, DataStreamReader reader,
        ref DataStreamReader.Context ctx, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_CubeSnapshotDataNewGhostIds.Add(ghostId);
                m_CubeSnapshotDataNewGhosts.Add(GhostReceiveSystem<NetworkingGhostDeserializerCollection>.InvokeSpawn<CubeSnapshotData>(snapshot, reader, ref ctx, compressionModel));
                break;
            case 1:
                m_SphereSnapshotDataNewGhostIds.Add(ghostId);
                m_SphereSnapshotDataNewGhosts.Add(GhostReceiveSystem<NetworkingGhostDeserializerCollection>.InvokeSpawn<SphereSnapshotData>(snapshot, reader, ref ctx, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<CubeSnapshotData> m_CubeSnapshotDataFromEntity;
    private NativeList<int> m_CubeSnapshotDataNewGhostIds;
    private NativeList<CubeSnapshotData> m_CubeSnapshotDataNewGhosts;
    private BufferFromEntity<SphereSnapshotData> m_SphereSnapshotDataFromEntity;
    private NativeList<int> m_SphereSnapshotDataNewGhostIds;
    private NativeList<SphereSnapshotData> m_SphereSnapshotDataNewGhosts;
}
public struct EnableNetworkingGhostReceiveSystemComponent : IComponentData
{}
public class NetworkingGhostReceiveSystem : GhostReceiveSystem<NetworkingGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableNetworkingGhostReceiveSystemComponent>();
    }
}
