using System.Collections.Generic;
using UnityEngine;

namespace WhoDunBit.Surfaces
{
    public static class Surfaces
    {
        private const SurfaceID DefaultID = SurfaceID.Rock;

        private static Dictionary<SurfaceID, SurfaceData> _dataByID;

        public static SurfaceData GetData(SurfaceID id = SurfaceID.Unknown)
        {
            // Load surface data once, when first requested
            if (_dataByID == null)
            {
                _dataByID = new();

                foreach (SurfaceData data in Resources.LoadAll<SurfaceData>("Data/Surfaces"))
                {
                    if (data.ID == SurfaceID.Unknown)
                    {
                        Debug.LogError($"Surface data '{data.name}' has no ID assigned!", data);
                        continue;
                    }

                    if (!_dataByID.TryAdd(data.ID, data))
                    {
                        Debug.LogError($"Duplicate surface ID '{data.ID}' on '{data.name}'!", data);
                    }
                }
            }

            if (!_dataByID.TryGetValue(id, out SurfaceData result))
            {
                _dataByID.TryGetValue(DefaultID, out result);
            }

            return result;
        }

        public static SurfaceData GetDefault()
        {
            return GetData(DefaultID);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            // Clear cached data when starting a new play session
            _dataByID = null;
        }
    }
}
