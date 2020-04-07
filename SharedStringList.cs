using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedStringList : SharedVariable<List<string>>
    {
        public static implicit operator SharedStringList(List<string> value) { return new SharedStringList { mValue = value }; }
    }
}