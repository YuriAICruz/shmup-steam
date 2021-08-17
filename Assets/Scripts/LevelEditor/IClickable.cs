using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene.LevelEditor
{
    public interface IClickable
    {
        Guid Id { get; }
        GameObject GameObject { get; }
        bool CanDelete { get; }
        string AssetName { get; }
        List<Tuple<string, object>> CustomData { get; }
        
        void Pick(Vector3Int pos);
        void Release(Vector3Int pos);
        void Drag(Vector3 pos);
        void Release();
        bool HasMenu();
        void UpdateData(List<Tuple<string, object>> rawData);
    }
}