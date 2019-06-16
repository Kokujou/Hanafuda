using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Hanafuda
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ConsultingMoveBuilder))]
    public class AIWeightEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ConsultingMoveBuilder moveBuilder = (ConsultingMoveBuilder)target;
            if (MainSceneVariables.boardTransforms == null) return;
            IArtificialIntelligence computer = (IArtificialIntelligence)MainSceneVariables.boardTransforms.Main.Players[1];
            Dictionary<string, float> weights = computer.GetWeights();
            foreach (var value in weights)
                computer.SetWeight(value.Key, EditorGUILayout.FloatField(value.Key, value.Value));
        }
    }
#endif
}
